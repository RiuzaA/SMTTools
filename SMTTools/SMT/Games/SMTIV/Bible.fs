module SMT.Games.SMTIV.Bible

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

type CompendiumUIMessage = { ID: byte; Text: string }

type CompendiumUIMessageStorer() =
    interface IStorable<CompendiumUIMessage> with
        member self.Read config reader =
            { ID = reader.ReadByte (); Text = decodeAtlusText config 0x53 reader }
        member self.Write config data writer =
            writer.EnsureSize 0x54 <| fun () ->
                writer.Write data.ID
                encodeZeroPaddedAtlusText config 0x53 data.Text writer
    interface ICSV<CompendiumUIMessage> with
        member self.CSVHeader _ _  = refCSVHeader<CompendiumUIMessage>
        member self.CSVRows _ data = [| refCSVRow data |]