module SMT.Games.SMTIVA.SkillData

open System.Runtime.InteropServices
open System.Text

open SMT.Types
open SMT.Utils

type SkillTarget = OneEnemy = 0x00uy | AllEnemies = 0x20uy | OneAlly = 0x40uy | AllAllies = 0x60uy | Self = 0x80uy | RandomEnemies = 0xC0uy | DemonInStock = 0xE0uy

type SkillType = Fire = 0x00uy | Ice = 0x01uy | Elec = 0x02uy | Force = 0x03uy | Almighty = 0x04uy | Light = 0x05uy | Dark = 0x06uy
               | Phys = 0x07uy | Gun = 0x08uy | Ailment = 0x09uy | Heal = 0x0Auy | Support = 0x0Buy | Special = 0x0Cuy

type SkillAilment = NoAil = 0x00uy | Death = 0x01uy | Brand = 0x03uy | Poison = 0x05uy | Panic = 0x06uy | Sleep = 0x07uy | Bind = 0x08uy | Sick = 0x09uy | Lost = 0x0Auy
                  | Charm = 0x0Buy | Mute = 0x0Duy

type Category = Physical = 0uy | Magic = 1uy | Ailment = 2uy | Heal = 3uy | Support = 4uy

type HitCount = {Min: byte; Max: byte}
module HitCount =
    let toString h = $"{h.Min} to {h.Max}"

type SkillData =
    { StrID:      string
      ID:         uint16
      MPCost:     byte 
      Category:   OrUnknown<Category, byte> // 0x23 
      Unknownx24: byte 
      Unknownx25: byte 
      Unknownx26: byte 
      Unknownx27: byte
      Target:     OrUnknown<SkillTarget, byte> // 0x28
      Type:       OrUnknown<SkillType, byte>   // 0x29
      Unknownx29: byte
      HitCount:   HitCount
      Unknownx2B: byte
      Power:      uint16 // 0x2C
      Accuracy:   byte   // 0x2D
      Ailment:    OrUnknown<SkillAilment, byte> // 0x2F, unsure about this
      AilmentAcc: byte // 0x30
      Unknownx31: byte array
      Zero:       uint16 }

type SkillDataStorer() =
    interface IStorable<SkillData> with
        member self.Read config reader =
            let strID        = reader.ReadZeroPaddedString 32
            let id           = reader.ReadUInt16()
            let mpCost       = reader.ReadByte()
            let category     = enumOrUnknown <| reader.ReadByte ()
            let unknownx24   = reader.ReadByte()
            let unknownx25   = reader.ReadByte()
            let unknownx26   = reader.ReadByte()
            let unknownx27   = reader.ReadByte()
            let bytex28      = reader.ReadByte()
            let unknownx29   = reader.ReadByte()
            let hitcountByte = reader.ReadByte()
            let unknownx2B   = reader.ReadByte()
            let power        = reader.ReadUInt16()
            let accuracy     = reader.ReadByte()
            let ailmentByte  = reader.ReadByte()
            let ailmentAcc   = reader.ReadByte()
            { StrID      = strID
              ID         = id
              MPCost     = mpCost
              Category   = category
              Unknownx24 = unknownx24
              Unknownx25 = unknownx25
              Unknownx26 = unknownx26
              Unknownx27 = unknownx27
              Target     = enumOrUnknown (bytex28 &&& 0xF0uy)
              Type       = enumOrUnknown (bytex28 &&& 0x0Fuy)
              Unknownx29 = unknownx29
              HitCount   = {Min = 0x0Fuy &&& hitcountByte; Max = (0xF0uy &&& hitcountByte) >>> 4}
              Unknownx2B = unknownx2B
              Power      = power
              Accuracy   = accuracy
              Ailment    = enumOrUnknown ailmentByte
              AilmentAcc = ailmentAcc
              Unknownx31 = reader.ReadBytes(0x88 - 0x31 - 0x2)
              Zero       = reader.ReadUInt16()}
        member self.Write config data writer =
            failwith "Unimplemented"

    interface ICSV<SkillData> with
        member self.CSVHeader _ _ = refCSVHeader<SkillData>
        member self.CSVRows _ data =
            [|[| "\"" + String.sanitizeControlChars data.StrID + "\""
                 data.ID.ToString()
                 data.MPCost.ToString()
                 OrUnknown.simpleToString data.Category
                 (int data.Unknownx24).ToString("X2")
                 (int data.Unknownx25).ToString("X2")
                 (int data.Unknownx26).ToString("X2")
                 (int data.Unknownx27).ToString("X2")
                 OrUnknown.simpleToString data.Target
                 OrUnknown.simpleToString data.Type
                 (int data.Unknownx29).ToString("X2")
                 HitCount.toString data.HitCount
                 (int data.Unknownx2B).ToString("X2")
                 data.Power.ToString()
                 data.Accuracy.ToString() + "%"
                 OrUnknown.simpleToString data.Ailment
                 (int data.AilmentAcc).ToString() + "%"
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx31)
                 data.Zero.ToString() |]|]