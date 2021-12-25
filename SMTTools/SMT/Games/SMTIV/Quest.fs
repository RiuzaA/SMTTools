module SMT.Games.SMTIV.Quest

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

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