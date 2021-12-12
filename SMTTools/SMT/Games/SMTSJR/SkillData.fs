module SMT.Games.SMTSJR.SkillData

open System
open System.Text

open SMT.Types
open SMT.Utils

type Category = Physical = 0x0uy | Magical = 0x01uy | Status = 0x02uy | Recovery = 0x03uy | Buff = 0x04uy | Summon = 0x07uy | Special = 0x08uy

type Target = OneFoe = 0x0uy | AllFoes = 0x02uy | OneAlly = 0x04uy | AllAllies = 0x06uy | All = 0x0Auy | RandomFoes = 0x0Cuy | OneDeadAlly = 0x0Euy

type Element = Fire = 0x0uy | Ice = 0x01uy | Elec = 0x02uy | Wind = 0x03uy | Almighty = 0x04uy | Expel = 0x05uy | Curse = 0x06uy | Phys = 0x07uy | Gun = 0x09uy
             | Defend = 0x0Duy | Ailment = 0x0Euy | Heal = 0x0Fuy | Support = 0x10uy | Special = 0x11uy | Talk = 0x12uy | Passive = 0x13uy

type Ailment = None = 0x00uy | Death = 0x01uy | Sleep = 0x02uy | Poison = 0x03uy | Paralyze = 0x04uy | Charm = 0x05uy | Mute = 0x06uy | Stone = 0x07uy
             | Fear = 0x08uy | Strain = 0x09uy | Bomb = 0x0Auy | Rage = 0x0Buy | AncientCurse = 0x0Cuy | RandomAilments = 0x0Fuy

type StatChange = ResetBoth (*0xF9*) | ResetBuff (*0xFA*) | ResetDebuff (*0xFB*) | ChangeBy of sbyte

module StatChange =
    let fromSByte = function
        | 0xF9y -> ResetBoth
        | 0xFAy -> ResetBuff
        | 0xFBy -> ResetDebuff
        | e     -> ChangeBy e

    let toSByte = function
        | ResetBoth   -> 0xF9y
        | ResetBuff   -> 0xFAy
        | ResetDebuff -> 0xFBy
        | ChangeBy b  -> b

type SkillData =
    { StrID:         string
      ID:            uint16
      MPCost:        byte 
      Category:      OrUnknown<Category, byte> // 0x23 
      Element:       Element                   // 0x24
      Target:        OrUnknown<Target, byte>   // 0x24
      Unknownx25:    byte
      Unknownx26:    byte
      Unknownx27:    byte
      Power:         uint16     // 0x28
      Accuracy:      byte       // 0x2A
      Ailment:       Ailment    // 0x2B, unsure about this
      AilmentAcc:    byte       // 0x2C
      Unknownx2D:    byte       // 0x2D
      AttackChange:  StatChange
      DefenseChange: StatChange
      AgilityChange: StatChange
      Unknownx31:    byte array // 0x31
      Zero:          uint16     // 0x4A 
      }

type PassiveElement = Phys = 0x01uy | Gun = 0x09uy | Fire = 0x11uy | Ice = 0x19uy | Elec = 0x21uy | Wind = 0x29uy | Expel = 0x31uy | Curse = 0x39uy | Recovery = 0x99uy

[<Flags>]
type PassiveFlags = Resist = 0x01us | Null = 0x02us | Repel = 0x03us | Drain = 0x04us | Boost = 0x05us | Amp = 0x06us | Pierce = 0x10us
                  | NullMind1  = 0x0040us | NullMind2  = 0x0200us | NullMind3  = 0x0400us | NullMind4  = 0x1000us | NullMind5 = 0x2000us
                  | NullNerve1 = 0x0080us | NullNerve2 = 0x0100us | NullNerve3 = 0x0800us | NullNerve4 = 0x4000us

type ElementalPassive = {Element: OrUnknown<PassiveElement, byte>; Flags: uint16}

type PassiveEffect =
    | ElementalPassive of ElementalPassive

type PassiveData =
    { StrID:           string // 0x00
      ID:              uint16 // 0x20
      Zero:            uint16 // 0x22
      MaxHPRank:       byte   // 0x24
      MaxMPRank:       byte   // 0x25
      CounterChance:   byte   // 0x26
      RecoverEachTurn: byte   // 0x27
      // RecoverEachTurn: byte   // 0x28
      }

type SkillDataStorer() =
    interface IStorable<SkillData> with
        member self.Read config reader =
            let strID    = reader.ReadZeroPaddedString 32
            let id       = reader.ReadUInt16()
            let mpCost   = reader.ReadByte()
            let category = enumOrUnknown <| reader.ReadByte ()
            let tarElem  = reader.ReadByte ()
            { StrID         = strID
              ID            = id
              MPCost        = mpCost
              Category      = category
              Element       = enumOrUnknown (0b00011111uy &&& tarElem) |> OrUnknown.unwrapOrErr
              Target        = enumOrUnknown ((0b11100000uy &&& tarElem) >>> 4)
              Unknownx25    = reader.ReadByte ()
              Unknownx26    = reader.ReadByte ()
              Unknownx27    = reader.ReadByte ()
              Power         = reader.ReadUInt16 ()
              Accuracy      = reader.ReadByte ()
              Ailment       = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              AilmentAcc    = reader.ReadByte ()
              Unknownx2D    = reader.ReadByte ()
              AttackChange  = StatChange.fromSByte <| reader.ReadSByte ()
              DefenseChange = StatChange.fromSByte <| reader.ReadSByte ()
              AgilityChange = StatChange.fromSByte <| reader.ReadSByte ()
              Unknownx31    = reader.ReadBytes <| 0x4A - 0x31
              Zero          = reader.ReadUInt16 () }
        member self.Write config data writer =
            failwith "Unimplemented"

    interface ICSV<SkillData> with
        member self.CSVHeader _ _ = refCSVHeader<SkillData>
        member self.CSVRows _ data =
            let printChange = function | ChangeBy b -> b.ToString(); | e -> e.ToString()
            [|[| "\"" + String.sanitizeControlChars data.StrID + "\""
                 data.ID.ToString()
                 data.MPCost.ToString()
                 OrUnknown.simpleToString data.Category
                 data.Element.ToString()
                 OrUnknown.simpleToString data.Target
                 (int data.Unknownx25).ToString("X2")
                 (int data.Unknownx26).ToString("X2")
                 (int data.Unknownx27).ToString("X2")
                 data.Power.ToString()
                 data.Accuracy.ToString()
                 data.Ailment.ToString()
                 (int data.AilmentAcc).ToString() + "%"
                 (int data.Unknownx2D).ToString("X2")
                 printChange data.AttackChange
                 printChange data.DefenseChange
                 printChange data.AgilityChange
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx31)
                 data.Zero.ToString() |]|]