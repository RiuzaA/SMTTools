module SMT.Formats.TBCR

open Newtonsoft.Json
open System.Collections.Generic
open System.IO
open System.Runtime.InteropServices
open System.Text

open SMT.TypeMap
open SMT.Types
open SMT.Utils

// Used in SMTIV and SMTIV:Apocalypse

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
type TBCRHeader =
    struct
        val MagicNum:     uint32
        val HeaderLength: uint32
        val EntryCount:   uint32

        [<JsonConstructor>]
        new(magicNum: uint32, headerLength: uint32, entryCount: uint32) = {MagicNum = magicNum; HeaderLength = headerLength; EntryCount = entryCount}
    end

type TBCREntry =
    | TBCRMSG of SMT.Formats.MSG.MSG
    | TBCRTBL of SMT.Formats.TBL.TBL

type TBCR =
    { Header:  TBCRHeader
      Offsets: uint32 array // from end of header
      Entries: TBCREntry array }

type TBCRStorer() =
    static let mkEntryStorer c s = storableFormat s, fun config -> c << iRead config s
    static let validEntryStorers = [ mkEntryStorer TBCRMSG <| SMT.Formats.MSG.MSGStorer.Instance
                                     mkEntryStorer TBCRTBL <| SMT.Formats.TBL.TBLStorer.Instance ]
    static let instance = TBCRStorer()
    static member Instance with get () = instance
    interface IHeader with
        member self.IsHeaderMatching _ reader = isHeaderEq "TBCR" reader
    interface IStorable<TBCR> with
        member self.Read config reader = 
            let startPos = reader.BaseStream.Position
            let header = reader.ReadStruct<TBCRHeader>()
            let offsets = Array.replicatef (int header.EntryCount) <| fun () -> reader.ReadUInt32()
            let entryStart = int64 header.HeaderLength + startPos
            reader.JumpTo entryStart
            let mutable entries = List.empty
            for idx = 0 to (int header.EntryCount)-1 do
                reader.JumpTo <| int64 offsets.[idx] + entryStart
                match List.tryFind (fun (fmt, read) -> (fmt :> IHeader).IsHeaderMatching config reader) validEntryStorers with
                | None ->
                    let possibleHeader = reader.ReadBytes 8
                    reader.ShiftPosition -8L
                    invalidOp $"Unknown TBCR entry header `{Encoding.ASCII.GetString(possibleHeader)}`"
                | Some (fmt, read) ->
                    entries <- read {config with Context = {config.Context with SubFileIdx = idx}} reader :: entries
            {Header = header; Offsets = offsets; Entries = List.toArray <| List.rev entries}

        member self.Write config data writer =
            let startPos = writer.BaseStream.Position
            let headerLen = max data.Header.HeaderLength (uint32 <| data.Entries.Length * 4 + 12)
            writer.Write data.Header.MagicNum
            writer.Write headerLen
            writer.Write (uint32 data.Entries.Length)
            let offsetPos = writer.BaseStream.Position
            // write dummy offsets
            for _ = 0 to (int data.Entries.Length)-1 do
                writer.Write 0xFFFFFFFFu
            // some files pad out the header; if we aren't yet at that point but it has padding, insert 0 bytes
            let padding = int headerLen - 12 - data.Entries.Length * 4
            for _ = 0 to padding-1 do
                writer.Write 0uy
            let entryStart = writer.BaseStream.Position
            for i = 0 to data.Entries.Length-1 do
                let entryStartPos = writer.BaseStream.Position
                let entry = data.Entries.[i]
                let getStorer e =
                    match findFirstWritableFormat config e with
                    | None -> failwith $"No format found for storing file of type {entry.GetType()}"
                    | Some storer -> storer 
                let entryPos = writer.BaseStream.Position
                writer.JumpTo <| offsetPos + 4L * int64 i
                writer.Write (uint32 (entryPos - entryStart))
                writer.JumpTo entryPos
                match entry with
                | TBCRMSG msg -> iWrite config (getStorer msg) (msg :> obj) writer
                | TBCRTBL tbl -> iWrite config (getStorer tbl) (tbl :> obj) writer
                let totalSize = int <| writer.BaseStream.Position - entryStartPos
                // TBCR entries are padded with zeros so they have a 16-byte alignment
                if totalSize % 16 <> 0 then
                    writer.PadZeros (16 - totalSize % 16)
    
    interface IManyCSV<TBCR> with
        member self.WriteCSVFiles config data path =
            let map = TypedMap<int>()
            for i in 0..data.Entries.Length-1 do
                match data.Entries.[i] with
                | TBCRMSG msg -> map.Set i msg
                | TBCRTBL tbl -> map.Set i tbl
            let sameFileValues = map.ToImmutable ()
            // following won't typecheck for some bizarre reason
            //let sameFileValues = Array.foldi (fun map idx -> function | TBCRMSG msg -> map.Set idx msg; | TBCRTBL tbl -> map.Set idx tbl) (ImmutableTypedMap<int>()) data.Entries
            let config' = {config with Context = {config.Context with SameFileValues = sameFileValues}}
            let mutable fileID = 0
            for entry in data.Entries do
                let (data, csv): obj * ICSV<obj> =
                    match entry with
                    | TBCRMSG msg -> msg :> obj, (config'.Game.CSVConverters.GetForOrThrow<obj> msg) :> ICSV<obj>
                    | TBCRTBL tbl -> tbl :> obj, (config'.Game.CSVConverters.GetForOrThrow<obj> tbl) :> ICSV<obj>
                let headerStr = System.String.Join(',', csv.CSVHeader config' data)
                let rowsStr   = System.String.Join('\n', Array.map (fun row -> System.String.Join(',', row :> IEnumerable<string>)) <| csv.CSVRows config' data)
                let csvBytes = Encoding.UTF8.GetBytes($"{headerStr}\n{rowsStr}")
                use writer = config.Context.GetFileWriter $"{path}/{config'.Context.BaseFileName}.{fileID}.csv"
                writer.Write csvBytes
                fileID <- fileID + 1
