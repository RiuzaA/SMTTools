module SMT.Games.SMTIV.Skill

open SMT.Formats.Text
open SMT.Types
open SMT.Utils

type SkillTarget = OneEnemy = 0x00uy | AllEnemies = 0x20uy | OneAlly = 0x40uy | AllAllies = 0x60uy | Self = 0x80uy | RandomEnemies = 0xC0uy | DemonInStock = 0xE0uy

type SkillType = Fire = 0x00uy | Ice = 0x01uy | Elec = 0x02uy | Force = 0x03uy | Almighty = 0x04uy | Light = 0x05uy | Dark = 0x06uy
               | Phys = 0x07uy | Gun = 0x08uy | Ailment = 0x09uy | Heal = 0x0Auy | Support = 0x0Buy | Special = 0x0Cuy

type SkillAilment = NoAil = 0x00uy | Death = 0x01uy | Brand = 0x03uy | Poison = 0x05uy | Panic = 0x06uy | Sleep = 0x07uy | Bind = 0x08uy | Sick = 0x09uy | Lost = 0x0Auy
                  | Charm = 0x0Buy | Mute = 0x0Duy

type Category = Physical = 0uy | Magic = 1uy | Ailment = 2uy | Heal = 3uy | Support = 4uy

type SkillData =
    { StrID:         string     // 0x00
      ID:            uint16     // 0x20
      MPCost:        byte       // 0x22
      Category:      OrUnknown<Category, byte>   // 0x23
      Unknownx24:    byte array // 0x24
    }

type SkillDataStorer() =
    interface IStorable<SkillData> with
        member self.Read config reader =
            { StrID      = decodeAtlusText config 0x20 reader
              ID         = reader.ReadUInt16 ()
              MPCost     = reader.ReadByte ()
              Category   = reader.ReadByte () |> enumOrUnknown //|> OrUnknown.unwrapOrErr
              Unknownx24 = reader.ReadBytes <| 0x68 - 0x24 }
        member self.Write config data writer =
            writer.EnsureSize 0x68 <| fun () ->
                encodeZeroPaddedAtlusText config 0x20 data.StrID writer
                writer.Write data.ID
                writer.Write data.MPCost
                writer.Write (OrUnknown.fromEnum data.Category)
                writer.Write data.Unknownx24
    interface ICSV<SkillData> with
        member self.CSVHeader _ _ = refCSVHeader<SkillData>
        member self.CSVRows _ data = [| refCSVRow data |]