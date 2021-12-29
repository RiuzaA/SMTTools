module SMT.Games.SMTIV.Quest

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

// NpcHaichiTable

type NPCHaichi =
    { Name:       string // 0x00
      Unknownx60: byte array // 0x74
      Zerox73:    byte array // 0x73
    }

type NPCHaichiStorer() =
    interface IStorable<NPCHaichi> with
        member self.Read config reader =
            { Name       = decodeAtlusText config 0x60 reader
              Unknownx60 = reader.ReadBytes <| 0x73 - 0x60
              Zerox73    = reader.ReadBytes <| 0x88 - 0x73}
        member self.Write config data writer =
            writer.EnsureSize 0x88 <| fun () ->
                encodeZeroPaddedAtlusText config 0x60 data.Name writer
                writer.Write data.Unknownx60
                writer.Write data.Zerox73
    interface ICSV<NPCHaichi> with
        member self.CSVHeader _ _ = refCSVHeader<NPCHaichi>
        member self.CSVRows _ data = [| refCSVRow data |]

// QcName

type QCMessage = {Name: string}

type QCMessageStorer() =
    interface IStorable<QCMessage> with
        member self.Read config reader =
            { Name = decodeAtlusText config 0x40 reader }
        member self.Write config data writer =
            writer.EnsureSize 0x40 <| fun () ->
                encodeZeroPaddedAtlusText config 0x40 data.Name writer
    interface ICSV<QCMessage> with
        member self.CSVHeader _ _ = refCSVHeader<QCMessage>
        member self.CSVRows _ data = [| refCSVRow data |]
        
type QuestLocation = QCMessage
type QuestLocationStorer = QCMessageStorer

// RankingTable

type RankingCategory = {Name: string}

type RankingCategoryStorer() =
    interface IStorable<RankingCategory> with
        member self.Read config reader =
            { Name = decodeAtlusText config 0x20 reader }
        member self.Write config data writer =
            writer.EnsureSize 0x20 <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.Name writer
    interface ICSV<RankingCategory> with
        member self.CSVHeader _ _ = refCSVHeader<RankingCategory>
        member self.CSVRows _ data = [| refCSVRow data |]

type HunterRanking =
    { Name:       string     // 0x00
      Unknownx16: byte array // 0x16 
    }

type HunterRankingStorer() =
    interface IStorable<HunterRanking> with
        member self.Read config reader =
            { Name = decodeAtlusText config 0x16 reader
              Unknownx16 = reader.ReadBytes <| 0x20 - 0x16 }
        member self.Write config data writer =
            writer.EnsureSize 0x20 <| fun () ->
                encodeZeroPaddedAtlusText config 0x16 data.Name writer
                writer.Write data.Unknownx16
    interface ICSV<HunterRanking> with
        member self.CSVHeader _ _ = refCSVHeader<HunterRanking>
        member self.CSVRows _ data = [| refCSVRow data |]

// QuestData & SubQuestData

type QuestData =
    { ID:          uint16     // 0x000
      Unknownx002: byte array // 0x002
      NameShort:   string     // 0x008
      NameLong:    string     // 0x038
      QuestGiver:  string     // 0x0B8
      Description: string     // 0x0E8
      Unknownx128: byte array // 0x128
    }

type QuestDataStorer() =
    interface IStorable<QuestData> with
        member self.Read config reader =
            { ID          = reader.ReadUInt16 ()
              Unknownx002 = reader.ReadBytes <| 0x008 - 0x002
              NameShort   = decodeAtlusText config 0x30 reader
              NameLong    = decodeAtlusText config 0x80 reader
              QuestGiver  = decodeAtlusText config 0x30 reader
              Description = decodeAtlusText config 0x40 reader
              Unknownx128 = reader.ReadBytes <| 0x1B4 - 0x128 }
        member self.Write config data writer =
            writer.EnsureSize 0x1B4 <| fun () ->
                writer.Write data.ID
                writer.Write data.Unknownx002
                encodeZeroPaddedAtlusText config 0x30 data.NameShort writer
                encodeZeroPaddedAtlusText config 0x80 data.NameLong writer
                encodeZeroPaddedAtlusText config 0x30 data.QuestGiver writer
                encodeZeroPaddedAtlusText config 0x40 data.Description writer
                writer.Write data.Unknownx128
    interface ICSV<QuestData> with
        member self.CSVHeader _ _ = refCSVHeader<QuestData>
        member self.CSVRows _ data = [| refCSVRow data |]

type QuestReward =
    { Name:       string     // 0x00
      Unknownx30: byte array // 0x30
    }

type QuestRewardStorer() =
    interface IStorable<QuestReward> with
        member self.Read config reader =
            { Name       = decodeAtlusText config (0x30 - 0x00) reader
              Unknownx30 = reader.ReadBytes <| 0x50 - 0x30 }
        member self.Write config data writer =
            writer.EnsureSize 0x50 <| fun () ->
                encodeZeroPaddedAtlusText config 0x30 data.Name writer
                writer.Write data.Unknownx30
    interface ICSV<QuestReward> with
        member self.CSVHeader _ _ = refCSVHeader<QuestReward>
        member self.CSVRows _ data = [| refCSVRow data |]