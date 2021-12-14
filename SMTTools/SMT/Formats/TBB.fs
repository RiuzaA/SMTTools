module SMT.Formats.TBB

open Newtonsoft.Json
open System.Collections.Generic
open System.IO
open System.Runtime.InteropServices
open System.Text

open SMT.Formats.MSG
open SMT.Formats.TBL
open SMT.TypeMap
open SMT.Types
open SMT.Utils

// Used in SMT:SJR

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
type TBBHeader =
    struct
        val MagicNum:     uint32
        val Unknown:      uint32 // always 10?
        val EntryCount:   uint32
        val FileSize:     uint32

        [<JsonConstructor>]
        new(magicNum: uint32, unknown: uint32, entryCount: uint32, fileSize: uint32) = {MagicNum = magicNum; Unknown = unknown; EntryCount = entryCount; FileSize = fileSize}
    end

type TBBEntry =
    | TBBMSG of MSG
    | TBBTBL of TBL

and TBB =
    { Header:  TBBHeader
      Offsets: uint32 array
      Entries: TBBEntry array }

type TBBStorer() =
    static let mkEntryReader constr storer (config: Config) reader =
        constr <| iRead config storer reader
    static let validEntryStorers = [ (storableFormat <| MSGStorer()), mkEntryReader TBBMSG MSGStorer.Instance
                                     (storableFormat <| TBLStorer()), mkEntryReader TBBTBL TBLStorer.Instance ]
    static let instance = TBBStorer()
    static member Instance with get () = instance
    interface IHeader with
        member self.IsHeaderMatching _ reader = isHeaderEq "TBB1" reader
    interface IStorable<TBB> with
        member self.Read config reader : TBB = 
            let startPos = reader.BaseStream.Position
            let header = reader.ReadStruct<TBBHeader>()
            let mutable entries = List.empty
            let offsets = Array.replicatef (int header.EntryCount) <| fun () -> reader.ReadUInt32()
            for idx = 0 to (int header.EntryCount)-1 do
                reader.JumpTo <| int64 offsets.[idx]
                match List.tryFind (fun (fmt, read) -> (fmt :> IHeader).IsHeaderMatching config reader) validEntryStorers with
                | None ->
                    invalidOp $"Unknown TBB entry header `{Encoding.ASCII.GetString(reader.ReadBytes 4)}`"
                | Some (fmt, read) ->
                    entries <- read {config with Context = {config.Context with SubFileIdx = idx}} reader :: entries
            {Header = header; Offsets = offsets; Entries = List.toArray <| List.rev entries}
        member self.Write config tbb writer =
            let startOffset = writer.BaseStream.Position
            writer.Write tbb.Header.MagicNum
            writer.Write tbb.Header.Unknown
            writer.Write (uint32 tbb.Entries.Length)

            let fileSizeOffset = writer.BaseStream.Position
            writer.Write 0xFFFFFFFFu // dummy value for now

            let offsetsOffset = writer.BaseStream.Position
            for _ in tbb.Entries do
                writer.Write 0xFFFFFFFFu

            // pad with zeros to preverse 16 byte alignment
            let headerSize = int (writer.BaseStream.Position - startOffset)
            if headerSize % 16 <> 0 then
                writer.PadZeros (16 - headerSize % 16)

            for i = 0 to tbb.Entries.Length-1 do
                let entryStartPos = writer.BaseStream.Position
                let entry = tbb.Entries.[i]
                let getStorer e =
                    match findFirstWritableFormat config e with
                    | None -> failwith $"No format found for storing file of type {entry.GetType()}"
                    | Some storer -> storer 
                let entryPos = writer.BaseStream.Position
                writer.JumpTo <| offsetsOffset + 4L * int64 i
                writer.Write (uint32 (entryPos - startOffset))
                writer.JumpTo entryPos
                match entry with
                | TBBMSG msg -> iWrite config (getStorer msg) (msg :> obj) writer
                | TBBTBL tbl -> iWrite config (getStorer tbl) (tbl :> obj) writer
                let totalSize = int <| writer.BaseStream.Position - entryStartPos
                // TBB entries are padded with zeros so they have a 16-byte alignment
                if totalSize % 16 <> 0 then
                    writer.PadZeros (16 - totalSize % 16)
            let endOffset = writer.BaseStream.Position
            let length = endOffset - startOffset
            writer.JumpTo fileSizeOffset
            writer.Write (uint32 length)
            writer.JumpTo endOffset

    interface IManyCSV<TBB> with
        member self.WriteCSVFiles config data path =
            let map = TypedMap<int>()
            for i in 0..data.Entries.Length-1 do
                match data.Entries.[i] with
                | TBBMSG msg -> map.Set i msg
                | TBBTBL tbl -> map.Set i tbl
            let sameFileValues = map.ToImmutable ()
            let config' = {config with Context = {config.Context with SameFileValues = sameFileValues}}
            let mutable fileID = 0
            
            for i in 0..data.Entries.Length-1 do
                let entry = data.Entries.[i]
                let (data, csv): obj * ICSV<obj> =
                    match entry with
                    | TBBMSG msg -> msg :> obj, (config'.Game.CSVConverters.GetForOrThrow<obj> msg) :> ICSV<obj>
                    | TBBTBL tbl -> tbl :> obj, (config'.Game.CSVConverters.GetForOrThrow<obj> tbl) :> ICSV<obj>
                let headerStr = System.String.Join(',', csv.CSVHeader config' data)
                let rowsStr   = System.String.Join('\n', Array.map (fun row -> System.String.Join(',', row :> IEnumerable<string>)) <| csv.CSVRows config' data)
                let csvBytes = Encoding.UTF8.GetBytes($"{headerStr}\n{rowsStr}")
                
                let key = {File = config.Context.BaseFileName; TableNum = i}
                let sectionName = Map.tryFind key config.Game.Sections |> Option.defaultValue (config.Context.SubFileIdx.ToString ())
                use stream = File.Create($"{path}/{config'.Context.BaseFileName}.{sectionName}.csv")
                stream.Write(csvBytes, 0, csvBytes.Length)
                fileID <- fileID + 1