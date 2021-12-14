module SMT.Games.SMTV.SkillData

open System
open System.Text

open SMT.Types
open SMT.Utils

type Target =
    | OneFoe     = 0x00uy
    | AllFoes    = 0x01uy
    | OneAlly    = 0x02uy
    | AllAllies  = 0x03uy
    | Self       = 0x04uy
    | RandomFoe  = 0x06uy
    | KOAlly     = 0x07uy
    | AllInStock = 0x08uy

type Category =
    | StrBased        = 0x00uy
    | MagBased        = 0x01uy
    | Ailment         = 0x02uy
    | Recovery        = 0x03uy
    | Buff            = 0x04uy
    | ReviveAndSummon = 0x07uy
    | Special         = 0x08uy
    | Summon          = 0x09uy
    | LevelDependent  = 0x0Duy
    | ScalesWithHP    = 0x0Euy
    | Unknownx15      = 0x0Fuy

type Element =
    | Physical = 0x00uy
    | Fire     = 0x01uy
    | Ice      = 0x02uy
    | Elec     = 0x03uy
    | Force    = 0x04uy
    | Light    = 0x05uy
    | Dark     = 0x06uy
    | Almighty = 0x07uy
    | Poison   = 0x08uy
    | Confuse  = 0x0Auy
    | Charm    = 0x0Buy
    | Sleep    = 0x0Cuy
    | Seal     = 0x0Duy
    | Mirage   = 0x14uy
    | NA       = 0x20uy

type Unknown1 =
    | Misc     = 0x00uy
    | Phys     = 0x01uy
    | Phys2    = 0x02uy
    | Fire     = 0x03uy
    | Ice      = 0x04uy
    | Elec     = 0x05uy
    | Force    = 0x06uy
    | Light    = 0x07uy
    | Dark     = 0x08uy
    | Almighty = 0x09uy

type Buff =
    | Maximize = -11           // 11110101
    | ResetDebuffAndRaiseBy1 = -10 // 11110110
    | Minimize1 = -9           // 11110111
    | ResetDebuff = -6         // 11111010
    | ResetBuff = -5           // 11111011
    | Minimize2 = -4           // 11111100
    | LowerBy2 = -2            // 11111110
    | LowerBy1 = -1            // 11111111
    | NA       = 0             // 00000000
    | RaiseBy1 = 1             // 00000001
    | RaiseBy2 = 2             // 00000010
    | Unknown4 = 4             // 00000100

type ResistChange =
    | NA       = 0x00uy
    | Null     = 0x02uy
    | Repel    = 0x03uy

type SkillData =
    { StrID:         string     // 0x00
      ID:            uint16     // 0x20
      Zerox22:       byte array // 0x22
      MPCost:        uint16     // 0x28
      Category:      Category   // 0x2A
      Unknownx2B:    Unknown1   // 0x2B
      Element1:      Element    // 0x2C
      Element2:      Element    // 0x2D
      Element3:      Element    // 0x2E
      Element4:      Element    // 0x2E
      Unknownx30:    byte array // 0x30
      Target:        Target     // 0x42
      Unknownx43:    byte       // 0x43
      MinHitCount:   uint8      // 0x44
      MaxHitCount:   uint8      // 0x45
      CritRateBonus: byte       // 0x46
      Unknownx47:    byte       // 0x47
      Power:         uint32     // 0x48
      Unknownx4C:    uint32     // 0x4C
      Unknownx50:    byte array // 0x50
      Accuracy:      uint16     // 0x54
      IsPoison:      bool       // 0x56
      Zerox4A:       byte       // 0x57
      IsPanic:       bool       // 0x58
      IsCharm:       bool       // 0x59
      IsSleep:       bool       // 0x5A
      IsSeal:        bool       // 0x5B
      Zerox4F:       uint16     // 0x5C
      IsMirage:      bool       // 0x5E
      Zerox52:       uint32     // 0x5F
      IsMud:         bool       // 0x63
      IsShroud:      bool       // 0x64
      Unknownx65:    byte       // 0x65
      Pierces:       bool       // 0x66
      Zerox67:       byte       // 0x67
      AilmentAcc:    byte       // 0x68
      Unknownx69:    byte       // 0x69
      Unknownx6A:    byte       // 0x6A
      Unknownx6B:    byte       // 0x6B
      AttackBuff1:   Buff       // 0x6C
      AttackBuff2:   Buff       // 0x70
      DefenseBuff:   Buff       // 0x74
      AgilityBuff:   Buff       // 0x78
      Unknownx7B:    uint32     // 0x7C
      ChangeResist:  bool       // 0x80
      PhysResist:    ResistChange // 0x81
      FireResist:    ResistChange // 0x82
      IceResist:     ResistChange // 0x83
      ElecResist:    ResistChange // 0x84
      ForceResist:   ResistChange // 0x85
      DarkResist:    ResistChange // 0x86
      LightResist:   ResistChange // 0x87
      Healing:       uint16     // 0x88
      Zerox8A:       uint16     // 0x8A
      Unknownx8C:    byte array // 0x8C
    }

type SkillDataStorer() =
    interface IStorable<SkillData> with
        member self.Read config reader =
            { StrID         = reader.ReadZeroPaddedString 0x20
              ID            = reader.ReadUInt16 ()
              Zerox22       = reader.ReadBytes <| 0x28 - 0x22
              MPCost        = reader.ReadUInt16 ()
              Category      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Unknownx2B    = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Element1      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Element2      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Element3      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Element4      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Unknownx30    = reader.ReadBytes <| 0x42 - 0x30
              Target        = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Unknownx43    = reader.ReadByte ()
              MinHitCount   = reader.ReadByte ()
              MaxHitCount   = reader.ReadByte ()
              CritRateBonus = reader.ReadByte ()
              Unknownx47    = reader.ReadByte ()
              Power         = reader.ReadUInt32 ()
              Unknownx4C    = reader.ReadUInt32 ()
              Unknownx50    = reader.ReadBytes <| 0x54 - 0x50
              Accuracy      = reader.ReadUInt16 ()
              IsPoison      = reader.ReadByte () = 1uy
              Zerox4A       = reader.ReadByte ()
              IsPanic       = reader.ReadByte () = 1uy
              IsCharm       = reader.ReadByte () = 1uy
              IsSleep       = reader.ReadByte () = 1uy
              IsSeal        = reader.ReadByte () = 1uy
              Zerox4F       = reader.ReadUInt16 ()
              IsMirage      = reader.ReadByte () = 1uy
              Zerox52       = reader.ReadUInt32 ()
              IsMud         = reader.ReadByte () = 1uy
              IsShroud      = reader.ReadByte () = 1uy
              Unknownx65    = reader.ReadByte ()
              Pierces       = reader.ReadByte () = 1uy
              Zerox67       = reader.ReadByte ()
              AilmentAcc    = reader.ReadByte ()
              Unknownx69    = reader.ReadByte ()
              Unknownx6A    = reader.ReadByte ()
              Unknownx6B    = reader.ReadByte ()
              AttackBuff1   = reader.ReadInt32 () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              AttackBuff2   = reader.ReadInt32 () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              DefenseBuff   = reader.ReadInt32 () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              AgilityBuff   = reader.ReadInt32 () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Unknownx7B    = reader.ReadUInt32 ()
              ChangeResist  = reader.ReadByte () = 1uy
              PhysResist    = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              FireResist    = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              IceResist     = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              ElecResist    = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              ForceResist   = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              DarkResist    = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              LightResist   = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Healing       = reader.ReadUInt16 ()
              Zerox8A       = reader.ReadUInt16 ()
              Unknownx8C    = reader.ReadBytes <| 0xC0 - 0x8C }
        member self.Write config data writer =
            writer.EnsureSize 0xC0 <| fun () ->
                writer.WriteZeroPaddedString 0x20 data.StrID
                writer.Write data.ID
                writer.Write data.Zerox22
                writer.Write data.MPCost
                writer.WriteEnum data.Category
                writer.WriteEnum data.Unknownx2B
                writer.WriteEnum data.Element1
                writer.WriteEnum data.Element2
                writer.WriteEnum data.Element3
                writer.WriteEnum data.Element4
                writer.Write data.Unknownx30
                writer.WriteEnum data.Target
                writer.Write data.Unknownx43
                writer.Write data.MinHitCount
                writer.Write data.MaxHitCount
                writer.Write data.CritRateBonus
                writer.Write data.Unknownx47
                writer.Write data.Power
                writer.Write data.Unknownx4C
                writer.Write data.Unknownx50
                writer.Write data.Accuracy
                writer.Write data.IsPoison
                writer.Write data.Zerox4A
                writer.Write data.IsPanic
                writer.Write data.IsCharm
                writer.Write data.IsSleep
                writer.Write data.IsSeal
                writer.Write data.Zerox4F
                writer.Write data.IsMirage
                writer.Write data.Zerox52
                writer.Write data.IsMud
                writer.Write data.IsShroud
                writer.Write data.Unknownx65
                writer.Write data.Pierces
                writer.Write data.Zerox67
                writer.Write data.AilmentAcc
                writer.Write data.Unknownx69
                writer.Write data.Unknownx6A
                writer.Write data.Unknownx6B
                writer.WriteEnumInt32 data.AttackBuff1
                writer.WriteEnumInt32 data.AttackBuff2
                writer.WriteEnumInt32 data.DefenseBuff
                writer.WriteEnumInt32 data.AgilityBuff
                writer.Write data.Unknownx7B
                writer.Write data.ChangeResist
                writer.WriteEnum data.PhysResist
                writer.WriteEnum data.FireResist
                writer.WriteEnum data.IceResist
                writer.WriteEnum data.ElecResist
                writer.WriteEnum data.ForceResist
                writer.WriteEnum data.DarkResist
                writer.WriteEnum data.LightResist
                writer.Write data.Healing
                writer.Write data.Zerox8A
                writer.Write data.Unknownx8C
            
    interface ICSV<SkillData> with
        member self.CSVHeader _ _ = refCSVHeader<SkillData>
        member self.CSVRows _ data =
            [|[| "\"" + String.sanitizeControlChars data.StrID + "\""
                 data.ID.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Zerox22) 
                 (int data.MPCost).ToString()
                 data.Category.ToString()
                 data.Unknownx2B.ToString()
                 data.Element1.ToString()
                 data.Element2.ToString()
                 data.Element3.ToString()
                 data.Element4.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx30) 
                 data.Target.ToString()
                 (int data.Unknownx43).ToString("X2")
                 (int data.MinHitCount).ToString()
                 (int data.MaxHitCount).ToString()
                 (int data.CritRateBonus).ToString() + "%"
                 (int data.Unknownx47).ToString("X2")
                 (int data.Power).ToString()
                 (int data.Unknownx4C).ToString("X2")
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx50) 
                 (int data.Accuracy).ToString() + "%"
                 data.IsPoison.ToString()
                 (int data.Zerox4A).ToString("X2")
                 data.IsPanic.ToString()
                 data.IsCharm.ToString()
                 data.IsSleep.ToString()
                 data.IsSeal.ToString()
                 (int data.Zerox4F).ToString("X2")
                 data.IsMirage.ToString()
                 (int data.Zerox52).ToString("X2")
                 data.IsMud.ToString()
                 data.IsShroud.ToString()
                 (int data.Unknownx65).ToString("X2")
                 data.Pierces.ToString()
                 (int data.Zerox67).ToString("X2")
                 data.AilmentAcc.ToString() + "%"
                 (int data.Unknownx69).ToString("X2")
                 (int data.Unknownx6A).ToString("X2")
                 (int data.Unknownx6B).ToString("X2")
                 data.AttackBuff1.ToString()
                 data.AttackBuff2.ToString()
                 data.DefenseBuff.ToString()
                 data.AgilityBuff.ToString()
                 (int data.Unknownx7B).ToString("X2")
                 data.ChangeResist.ToString()
                 data.PhysResist.ToString()
                 data.FireResist.ToString()
                 data.IceResist.ToString()
                 data.ElecResist.ToString()
                 data.ForceResist.ToString()
                 data.DarkResist.ToString()
                 data.LightResist.ToString()
                 data.Healing.ToString()
                 data.Zerox8A.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx8C) |]|]