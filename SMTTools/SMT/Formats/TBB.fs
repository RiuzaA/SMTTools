module SMT.Formats.TBB

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
            failwith "Unimplemented"
    
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