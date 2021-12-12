module SMT.Formats.TBL

open Newtonsoft.Json
open System.Runtime.InteropServices

open SMT.Types
open SMT.Utils

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
type TBLHeader =
    struct
        val MagicNum:  uint32
        val TableSize: uint32
        val RowSize:   uint32
        val Unknown:   uint32
        (* // in Strange Journey Redux
        val MagicNum:          uint32
        val Unknown:   uint32
        val TableSize: uint32
        val RowSize:   uint32
        *)
        [<JsonConstructor>]
        new(magicNum: uint32, tableSize: uint32, rowSize: uint32, unknown: uint32) = {MagicNum = magicNum; TableSize = tableSize; RowSize = rowSize; Unknown = unknown}
    end

// Needed to prevent Json.net from interpreting these as JArrays of strings instead of byte arrays
type BinaryTBLEntry = {HexBytes: byte array}

type TBLEntry = obj

and TBL =
    { Header:  TBLHeader
      Entries: TBLEntry array }

let toSMTSJRHeader (header: TBLHeader) =
    let unknown   = header.TableSize
    let tableSize = header.RowSize
    let rowSize   = header.Unknown
    new TBLHeader(header.MagicNum, tableSize, rowSize, unknown)

let fromSMTSJRHeader (header: TBLHeader) =
    let tableSize = header.Unknown
    let rowSize   = header.TableSize
    let unknown   = header.RowSize
    new TBLHeader(header.MagicNum, tableSize, rowSize, unknown)

type TBLStorer() =
    static let getConverter config data =
        match data.Entries with
        | [||]    -> None
        | entries -> Some (config.Game.CSVConverters.Get (entries.[0].GetType()))
    static let instance = TBLStorer()
    static member Instance with get () = instance
    interface IHeader with
        member self.IsHeaderMatching _ reader = isHeaderEq "TBL1" reader
    interface IStorable<TBL> with
        member self.Read config reader = 
            let startPos = reader.BaseStream.Position
            let mutable header = reader.ReadStruct<TBLHeader>()
            match config.Game.ID with
            | "SMTSJR" ->
                header <- toSMTSJRHeader header
            | "Unknown" ->
                if header.Unknown <> 0x10u && header.TableSize = 0x10u then // these are flipped in SMTIV/A, so we can guess its that format
                    header <- fromSMTSJRHeader header
                ()
            | _ -> ()
            let entryCount = int (header.TableSize / header.RowSize)
            let storer = Map.tryFind {File = config.Context.BaseFileName; TableNum = config.Context.SubFileIdx} config.Game.TableRowConverters
                      |> defaultArg
                      <| (int header.RowSize |> BytesStorer |> objStorable)
            let wrapIfBytes (e: obj) =
                match e with
                | :? array<byte> as arr -> {HexBytes = arr} :> obj
                | _ -> e
            let entries = Array.replicatef entryCount (fun idx -> wrapIfBytes <| storer.Read config reader)
            {Header = header; Entries = entries}

        member self.Write config data writer =
            let len = max (int data.Header.TableSize + 16) (16 + int data.Header.RowSize * data.Entries.Length)
            writer.EnsureSize len <| fun () ->
                let header' = new TBLHeader(data.Header.MagicNum, uint32 (len - 16), data.Header.RowSize, data.Header.Unknown)
                if config.Game.ID = "SMTSJR" then 
                    writer.WriteStruct <| fromSMTSJRHeader header'
                else
                    writer.WriteStruct header'
                for entry in data.Entries do
                    let entry' =
                        match entry with
                        | :? BinaryTBLEntry as e -> e.HexBytes :> obj
                        | _ -> entry
                    match findFirstWritableFormat config entry' with
                    | None -> failwith $"No format found for storing file of type {entry'.GetType()}"
                    | Some storer -> storer.Write config entry' writer

    // Serializes exactly as its contained rows, if any, serialize
    interface ICSV<TBL> with
        member self.CSVHeader config data =
            match getConverter config data with
            | None -> [||]
            | Some conv -> (conv :> ICSV<obj>).CSVHeader config data.Entries.[0]
        member self.CSVRows config data =
            match getConverter config data with
            | None -> [||]
            | Some conv -> Array.map (fun row -> (conv :> ICSV<obj>).CSVRows config row) data.Entries |> Array.concat