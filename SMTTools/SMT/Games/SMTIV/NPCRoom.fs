module SMT.Games.SMTIV.NPCRoom

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

// NPCRoomTable.tbb.[0]
type NPCRoomInfo1 =
    { ID:         uint16     // 0x00
      Name:       string     // 0x02
      Unknownx22: byte array // 0x22
    }

type NPCRoomInfo1Storer() =
    interface IStorable<NPCRoomInfo1> with
        member self.Read config reader =
            { ID         = reader.ReadUInt16 ()
              Name       = decodeAtlusText config 0x20 reader
              Unknownx22 = reader.ReadBytes <| 0x6C - 0x22 }
        member self.Write config data writer =
            writer.EnsureSize 0x6C <| fun () ->
                writer.Write data.ID
                encodeZeroPaddedAtlusText config 0x20 data.Name writer
                writer.Write data.Unknownx22
    interface ICSV<NPCRoomInfo1> with
        member self.CSVHeader _ _ = refCSVHeader<NPCRoomInfo1>
        member self.CSVRows _ data = [| refCSVRow data |]

// NPCRoomTable.tbb.[1]
type NPCRoomInfo2 =
    { Unknownx00:   uint16     // 0x00
      CharSpriteID: uint16     // 0x02
      Unknownx04:   byte array // 0x04
      NameShort:    string     // 0x26 (do not trust this size, since none got anywhere near max)
      NameLong:     string     // 0x54 (do not trust this size, since none got anywhere near max)
      ZeroxD2:      uint16     // 0xD2
    }

type NPCRoomInfo2Storer() =
    interface IStorable<NPCRoomInfo2> with
        member self.Read config reader =
            { Unknownx00   = reader.ReadUInt16 ()
              CharSpriteID = reader.ReadUInt16 ()
              Unknownx04   = reader.ReadBytes <| 0x26 - 0x04
              NameShort    = decodeAtlusText config 0x2E reader
              NameLong     = decodeAtlusText config 0x7E reader
              ZeroxD2      = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0xD4 <| fun () ->
                writer.Write data.Unknownx00
                writer.Write data.CharSpriteID
                writer.Write data.Unknownx04
                encodeZeroPaddedAtlusText config 0x2E data.NameShort writer
                encodeZeroPaddedAtlusText config 0x7E data.NameLong writer
                writer.Write data.ZeroxD2
    interface ICSV<NPCRoomInfo2> with
        member self.CSVHeader _ _ = refCSVHeader<NPCRoomInfo2>
        member self.CSVRows _ data = [| refCSVRow data |]