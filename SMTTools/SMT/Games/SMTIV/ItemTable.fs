module SMT.Games.SMTIV.ItemTable

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

// ItemTable.tbb.[0]
type KeyItem =
    { NameShort:   string     // 0x00
      NameLong:    string     // 0x20
      Unknownx50:  byte array // 0x50
      IsQuestItem: bool      // 0x53
      Unknownx54:  uint16     // 0x54
      Zerox56:     uint16     // 0x56
    }

// ItemTable.tbb.[1]
type ConsumableItem =
    { NameShort:  string     // 0x00
      NameLong:   string     // 0x20
      Unknownx50: byte array // 0x50
      MaxStack:   uint16     // 0x71
      Unknownx73: byte array // 0x73
      Price:      uint16     // 0x7C
      Zerox7E:    uint16     // 0x7E
    }

// ItemTable.tbb.[2]
type EquipmentItem =
    { NameShort:  string     // 0x00
      NameLong:   string     // 0x20
      Unknownx50: byte array // 0x50
      Power:      uint16     // 0x54
      Unknownx56: byte array // 0x56
      ZeroxA6:    uint16     // 0xA6
    }
    
// ItemTable.tbb.[3]
type RelicCategory =
    { Name:       string     // 0x00
      Unknownx20: byte array // 0x20
    }
    
// ItemTable.tbb.[4]
type RelicItem =
    { Name:       string     // 0x00
      Unknownx20: byte array // 0x20
      Price:      uint16     // 0x2C
      Zerox2E:    uint16     // 0x2E
    }

// ItemTable.tbb.[5]
type LegionSkill =
    { Name:       string
      Unknownx20: byte array // 0x20
      Zerox2A:    uint16     // 0x2A
    }

type KeyItemStorer() =
    interface IStorable<KeyItem> with
        member self.Read config reader =
            reader.EnsureSize 0x58 <| fun () ->
                { NameShort   = decodeAtlusText config 0x20 reader
                  NameLong    = decodeAtlusText config 0x30 reader
                  Unknownx50  = reader.ReadBytes <| 0x53 - 0x50
                  IsQuestItem = reader.ReadByte () = 0x0Fuy
                  Unknownx54  = reader.ReadUInt16 ()
                  Zerox56     = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0x58 <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.NameShort writer
                encodeZeroPaddedAtlusText config 0x30 data.NameLong writer
                writer.Write data.Unknownx50
                writer.Write (if data.IsQuestItem then 0x0Fuy else 0uy)
                writer.Write data.Unknownx54
                writer.Write data.Zerox56
    interface ICSV<KeyItem> with
        member self.CSVHeader _ _ = refCSVHeader<KeyItem>
        member self.CSVRows _ data = [| refCSVRow data |]
             
type ConsumableItemStorer() =    
    interface IStorable<ConsumableItem> with
        member self.Read config reader =
            reader.EnsureSize 0x80 <| fun () ->
                { NameShort  = decodeAtlusText config 0x20 reader
                  NameLong   = decodeAtlusText config 0x30 reader
                  Unknownx50 = reader.ReadBytes <| 0x71 - 0x50
                  MaxStack   = reader.ReadUInt16 ()
                  Unknownx73 = reader.ReadBytes <| 0x7C - 0x73
                  Price      = reader.ReadUInt16 ()
                  Zerox7E    = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0x80 <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.NameShort writer
                encodeZeroPaddedAtlusText config 0x30 data.NameLong writer
                writer.Write data.Unknownx50
                writer.Write data.MaxStack
                writer.Write data.Unknownx73
                writer.Write data.Price
                writer.Write data.Zerox7E
    interface ICSV<ConsumableItem> with
        member self.CSVHeader _ _ = refCSVHeader<KeyItem>
        member self.CSVRows _ data = [| refCSVRow data |]

type EquipmentItemStorer() =
    interface IStorable<EquipmentItem> with
        member self.Read config reader =
            reader.EnsureSize 0xA8 <| fun () ->
                { NameShort  = decodeAtlusText config 0x20 reader
                  NameLong   = decodeAtlusText config 0x30 reader
                  Unknownx50 = reader.ReadBytes <| 0x54 - 0x50
                  Power      = reader.ReadUInt16 ()
                  Unknownx56 = reader.ReadBytes <| 0xA6 - 0x56
                  ZeroxA6    = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0xA8 <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.NameShort writer
                encodeZeroPaddedAtlusText config 0x30 data.NameLong writer
                writer.Write data.Unknownx50
                writer.Write data.Power
                writer.Write data.Unknownx56
                writer.Write data.ZeroxA6
    interface ICSV<EquipmentItem> with
        member self.CSVHeader _ _ = refCSVHeader<EquipmentItem>
        member self.CSVRows _ data = [| refCSVRow data |]

type RelicCategoryStorer() =
    interface IStorable<RelicCategory> with
        member self.Read config reader =
            reader.EnsureSize 0x3C <| fun () ->
                { Name       = decodeAtlusText config 0x20 reader
                  Unknownx20 = reader.ReadBytes <| 0x3C - 0x20}
        member self.Write config data writer =
            writer.EnsureSize 0x3C <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.Name writer
                writer.Write data.Unknownx20
    interface ICSV<RelicCategory> with
        member self.CSVHeader _ _ = refCSVHeader<RelicCategory>
        member self.CSVRows _ data = [| refCSVRow data |]

type RelicItemStorer() =
    interface IStorable<RelicItem> with
        member self.Read config reader =
            reader.EnsureSize 0x30 <| fun () ->
                { Name       = decodeAtlusText config 0x20 reader
                  Unknownx20 = reader.ReadBytes <| 0x2C - 0x20
                  Price      = reader.ReadUInt16 ()
                  Zerox2E    = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0x30 <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.Name writer
                writer.Write data.Unknownx20
                writer.Write data.Price
                writer.Write data.Zerox2E
    interface ICSV<RelicItem> with
        member self.CSVHeader _ _ = refCSVHeader<RelicItem>
        member self.CSVRows _ data = [| refCSVRow data |]

type LegionSkillStorer() =
    interface IStorable<LegionSkill> with
        member self.Read config reader =
            reader.EnsureSize 0x2C <| fun () ->
                { Name       = decodeAtlusText config 0x20 reader
                  Unknownx20 = reader.ReadBytes <| 0x2A - 0x20
                  Zerox2A    = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0x2C <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.Name writer
                writer.Write data.Unknownx20
                writer.Write data.Zerox2A
    interface ICSV<LegionSkill> with
        member self.CSVHeader _ _ = refCSVHeader<LegionSkill>
        member self.CSVRows _ data = [| refCSVRow data |]