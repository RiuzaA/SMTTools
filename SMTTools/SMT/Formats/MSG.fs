module SMT.Formats.MSG

open Newtonsoft.Json
open System.Collections.Generic
open System.Runtime.InteropServices

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
type MSGHeader =
    struct
        val MagicNum:      uint64
        val Zerox08:       uint16
        val ConstOne:      uint16
        val FileSize:      uint32
        val EntryCount:    uint32
        val EntriesOffset: uint32
        val Zerox18:       uint64

        [<JsonConstructor>]
        new(magicNum: uint64, zerox08: uint16, constOne: uint16, fileSize: uint32, entryCount: uint32, entriesOffset: uint32, zerox18: uint64) =
            { MagicNum = magicNum
              Zerox08 = zerox08
              ConstOne = constOne
              FileSize = fileSize
              EntryCount = entryCount
              EntriesOffset = entriesOffset
              Zerox18 = zerox18 }
    end

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
type MSGEntry =
    struct
        val EntryID:    uint32
        val TextLength: uint32
        val TextOffset: uint32
        val Padding:    uint32

        [<JsonConstructor>]
        new(entryID: uint32, textLength: uint32, textOffset: uint32, padding: uint32) = {EntryID = entryID; TextLength = textLength; TextOffset = textOffset; Padding = padding}
    end

type MSG = MSG of Map<uint32, string>

type MSGStorer() =
    static let instance = MSGStorer()
    static member Instance with get () = instance
    
    interface IHeader with
        member self.IsHeaderMatching _ reader = isHeaderEq "\u0000\u0000\u0000\u0000MSG2" reader

    interface IStorable<MSG> with
        member self.Read config reader: MSG = 
            let startPos = reader.BaseStream.Position
            let header = reader.ReadStruct<MSGHeader>()
            let mutable entries = new List<MSGEntry>()
            for _ = 0 to (int header.EntryCount)-1 do
                entries.Add(reader.ReadStruct<MSGEntry>())
            let mutable entryMap = Map.empty
            for entry in entries do
                if entry.TextOffset <> 0u then
                    reader.BaseStream.Position <- startPos + int64 entry.TextOffset
                    let txt = decodeAtlusText config (int entry.TextLength) reader
                    entryMap <- Map.add entry.EntryID txt entryMap
            reader.BaseStream.Position <- startPos + int64 header.FileSize
            MSG entryMap

        member self.Write config (MSG msg) writer =
            let startPos = writer.BaseStream.Position
            writer.Write 0
            writer.Write 0x3247534D
            writer.Write 0s
            writer.Write 1s
            let sizePos = writer.BaseStream.Position
            writer.Write 0x12345678
            writer.Write (Map.count msg)
            writer.Write (Marshal.size<MSGHeader> + Marshal.size<MSGEntry> * Map.count msg)
            // entry
            let entryStartPos = writer.BaseStream.Position
            Map.iter (fun k v -> writer.WriteStruct (MSGEntry(k, 0u, 0u, 0u))) msg
            let writeStr k str =
                let strStartPos = writer.BaseStream.Position
                let len = encodeAtlusText config str writer
                writer.Write 0us
                let curPos = writer.BaseStream.Position
                writer.JumpTo (entryStartPos + int64 k * int64 Marshal.size<MSGEntry>)
                writer.WriteStruct (MSGEntry(k, uint32 len, uint32 strStartPos, 0u))
                writer.JumpTo curPos
            Map.iter writeStr msg
            let endPos = writer.BaseStream.Position
            writer.JumpTo sizePos
            writer.Write (endPos - startPos)
            writer.JumpTo endPos

    interface ICSV<MSG> with
        member self.CSVHeader _ _ = [|"Id"; "Text"|]
        member self.CSVRows _ (MSG data) = Array.map (fun (idx, txt) -> [| "\"" + idx.ToString() + "\""; txt|]) <| Map.toArray data