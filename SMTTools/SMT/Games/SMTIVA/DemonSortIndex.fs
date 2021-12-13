module SMT.Games.SMTIVA.DemonSortIndex

open SMT.Types
open SMT.Utils

// NKMSortIndex.tbb.[0]
type RaceName =
    { Name1: string
      Name2: string } // identical for all but posthuman / superhuman; not sure why

type RaceNameStorer() =
    interface IStorable<RaceName> with
        member self.Read config reader =
            let name1 = reader.ReadZeroPaddedString 16
            let name2 = reader.ReadZeroPaddedString 16
            reader.ShiftPosition 16L
            { Name1 = name1; Name2 = name2 }
        member self.Write config data writer =
            writer.EnsureSize 0x28 <| fun () ->
                writer.WriteZeroPaddedString 0x16 data.Name1
                writer.WriteZeroPaddedString 0x16 data.Name2
    interface ICSV<RaceName> with
        member self.CSVHeader _ _ = refCSVHeader<RaceName>
        member self.CSVRows config data =
            [|[| "\"" + String.sanitizeControlChars data.Name1 + "\""
                 "\"" + String.sanitizeControlChars data.Name2 + "\""|]|]

// NKMSortIndex.tbb.[1]
type ActorName =
    { Name: string }

type ActorNameStorer() =
    interface IStorable<ActorName> with
        member self.Read _ reader = { Name = reader.ReadZeroPaddedString 0x20 }
        member self.Write config data writer =
            writer.EnsureSize 0x20 <| fun () ->
                writer.WriteZeroPaddedString 0x20 data.Name
    interface ICSV<ActorName> with
        member self.CSVHeader _ _ = refCSVHeader<ActorName>
        member self.CSVRows config data =
            [|[| "\"" + String.sanitizeControlChars data.Name + "\"" |]|]