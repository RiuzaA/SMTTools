module SMT.Games.SMTIV.DemonSortIndex

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

// NKMSortIndex.tbb.[0]
type RaceName =
    { Name1: string
      Name2: string }

type RaceNameStorer() =
    interface IStorable<RaceName> with
        member self.Read config reader =
            let name1 = decodeAtlusText config 0x10 reader
            let name2 = decodeAtlusText config 0x18 reader
            { Name1 = name1; Name2 = name2 }
        member self.Write config data writer =
            writer.EnsureSize 0x28 <| fun () ->
                encodeZeroPaddedAtlusText config 0x10 data.Name1 writer
                encodeZeroPaddedAtlusText config 0x18 data.Name2 writer
    interface ICSV<RaceName> with
        member self.CSVHeader _ _ = refCSVHeader<RaceName>
        member self.CSVRows config data =
            [|[| "\"" + String.sanitizeControlChars data.Name1 + "\""
                 "\"" + String.sanitizeControlChars data.Name2 + "\""|]|]

// NKMSortIndex.tbb.[1]
type ActorName = SMT.Games.SMTIVA.DemonSortIndex.ActorName

type ActorNameStorer = SMT.Games.SMTIVA.DemonSortIndex.ActorNameStorer