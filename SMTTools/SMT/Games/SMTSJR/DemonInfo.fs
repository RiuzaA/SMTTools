module  SMT.Games.SMTSJR.DemonInfo

open Microsoft.FSharp.NativeInterop
open System
open System.IO
open SMT.Games.SMTSJR.SkillData
open SMT.Types
open SMT.Utils

type Race = Blank = 0x00uy | Herald = 0x01uy | Megami = 0x02uy | Avian = 0x03uy | Tree = 0x04uy | Divine = 0x05uy | Flight = 0x06uy | Yoma = 0x07uy
          | Nymph = 0x08uy | Vile = 0x09uy | Raptor = 0x0Auy | Wood = 0x0Buy | Deity = 0x0Cuy | Avatar = 0x0Duy | Holy = 0x0Euy | Genma = 0x0Fuy
          | Fairy = 0x10uy | Beast = 0x11uy | Jirae = 0x12uy | Snake = 0x13uy | Reaper = 0x14uy | Wilder = 0x15uy | Jaki = 0x16uy | Vermin = 0x17uy
          | Fury = 0x18uy | Lady = 0x19uy | Dragon = 0x1Auy | Kishin = 0x1Buy | Fallen = 0x1Cuy | Brute = 0x1Duy | Femme = 0x1Euy | Night = 0x1Fuy
          | Tyrant = 0x20uy | Drake = 0x21uy | Spirit = 0x22uy | Foul = 0x23uy | Haunt = 0x24uy | Mitama = 0x25uy | Prime = 0x26uy | Fiend = 0x27uy
          | Enigma = 0x28uy | UMA = 0x29uy | Zealot = 0x2Auy | Human = 0x2Buy | Cyber = 0x2Cuy | Meta = 0x2Duy | Witch = 0x2Euy | Root = 0x2Fuy
          | Geist = 0x30uy | Mother = 0x31uy | Empty = 0x32uy | Awake = 0x33uy | Soil = 0x34uy | Judge = 0x35uy | Pillar = 0x36uy | Fake = 0x37uy
          | RootDupl = 0x38uy
          
type ResistType = Normal = 0x00uy | Null = 0x04uy | Weak = 0x08uy | AilmentWeak = 0x09uy | Repel = 0x0Cuy | Drain = 0x10uy | Resist = 0x14uy | MastemaFear = 0x64uy

type Lightness = Light = 0x02uy | Neutral = 0x01uy | Dark = 0x03uy

type Alignment = Law = 0x04uy | Neutral = 0x01uy | Chaos = 0x05uy

[<Flags>]
type InheritType = MP = 0x8000us | HP = 0x4000us | Special = 0x2000us | Ailment = 0x1000us
                 | Gun = 0x800us | Phys = 0x400us | Curse = 0x200us | Expel = 0x100us
                 | Wind = 0x80us | Elec = 0x40us | Ice = 0x20us | Fire = 0x10us
                 | Support = 0x8us | Recovery = 0x4us | Almighty = 0x2us
                 | CanInherit = 0x1us | CannotInherit = 0x0us

type AttackElement = Phys = 0x00uy | Gun = 0x01uy

type ElementalResist =
    { Multiplier: byte
      Reaction:   ResistType }

[<CSVUnpack>]
type Resists =
    { Phys:     ElementalResist
      Gun:      ElementalResist
      Fire:     ElementalResist
      Ice:      ElementalResist
      Elec:     ElementalResist
      Wind:     ElementalResist
      Expel:    ElementalResist
      Curse:    ElementalResist
      Sleep:    ElementalResist
      Poison:   ElementalResist
      Paralyze: ElementalResist
      Charm:    ElementalResist
      Mute:     ElementalResist
      Stone:    ElementalResist
      Fear:     ElementalResist
      Strain:   ElementalResist
      Bomb:     ElementalResist
      Rage:     ElementalResist
      Almighty: ElementalResist
    }

type DemonInfo =
    { ID:             uint16                // 0x00
      Race:           OrUnknown<Race, byte> // 0x02
      Level:          byte                  // 0x03
      Lightness:      Lightness             // 0x04
      Alignment:      Alignment             // 0x05
      DSourceID:      uint16                // 0x06
      Inherits:       InheritType Set       // 0x08
      Unknownx0A:     byte                  // 0x0A
      AttackHitMax:   byte                  // 0x0B
      AttackElement:  AttackElement         // 0x0C
      Const100x0D:    byte                  // 0x0D
      Ailment:        Ailment               // 0x0E
      AilmentChance:  byte                  // 0x0F
      Unknownx10:     byte array            // 0x10
      Strength:       byte       // 0x1A
      Vitality:       byte       // 0x1B
      Agility:        byte       // 0x1C
      Luck:           byte       // 0x1D
      Magic:          byte       // 0x1E
      Unknownx1F:     byte array // 0x1F
      Skills:         uint16 array    // 0x2E, only 3 used in vanilla, but all 6 work just fine
      Resists:        Resists // 0x3A
      Unknownx60:     byte   // 0x60
      Unknownx61:     byte   // 0x61
      Unknownx62:     byte   // 0x62
      Unknownx63:     byte   // 0x63
      Unknownx64:     byte   // 0x64
      Unknownx65:     byte   // 0x65
      Zero:           uint16 // 0x66
      }

let readResists (reader: BinaryReader) =
    let readResist () = {Multiplier = reader.ReadByte (); Reaction = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr}
    { Phys     = readResist ()
      Gun      = readResist ()
      Fire     = readResist ()
      Ice      = readResist ()
      Elec     = readResist ()
      Wind     = readResist ()
      Expel    = readResist ()
      Curse    = readResist ()
      Sleep    = readResist ()
      Poison   = readResist ()
      Paralyze = readResist ()
      Charm    = readResist ()
      Mute     = readResist ()
      Stone    = readResist ()
      Fear     = readResist ()
      Strain   = readResist ()
      Bomb     = readResist ()
      Rage     = readResist ()
      Almighty = readResist () }

let writeResists (writer: BinaryWriter) data =
    let writeResist res =
            writer.Write res.Multiplier
            writer.Write (LanguagePrimitives.EnumToValue res.Reaction : byte)
    writeResist data.Phys
    writeResist data.Gun
    writeResist data.Fire
    writeResist data.Ice
    writeResist data.Elec
    writeResist data.Wind
    writeResist data.Expel
    writeResist data.Curse
    writeResist data.Sleep
    writeResist data.Poison
    writeResist data.Paralyze
    writeResist data.Charm
    writeResist data.Mute
    writeResist data.Stone
    writeResist data.Fear
    writeResist data.Strain
    writeResist data.Bomb
    writeResist data.Rage
    writeResist data.Almighty

type DemonInfoStorer() =
    interface IStorable<DemonInfo> with
        member self.Read config reader =
            let id       = reader.ReadUInt16 ()
            let race     = reader.ReadByte () |> enumOrUnknown
            let level    = reader.ReadByte ()
            { ID             = id
              Race           = race
              Level          = level
              Lightness      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Alignment      = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              DSourceID      = reader.ReadUInt16 ()
              Inherits       = reader.ReadUInt16 () |> LanguagePrimitives.EnumOfValue |> Flag.toSet
              Unknownx0A     = reader.ReadByte ()
              AttackHitMax   = reader.ReadByte ()
              AttackElement  = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Const100x0D    = reader.ReadByte ()
              Ailment        = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              AilmentChance  = reader.ReadByte ()
              Unknownx10     = reader.ReadBytes <| 0x1A - 0x10
              Strength       = reader.ReadByte ()
              Vitality       = reader.ReadByte ()
              Agility        = reader.ReadByte ()
              Luck           = reader.ReadByte ()
              Magic          = reader.ReadByte ()
              Unknownx1F     = reader.ReadBytes <| 0x2E - 0x1F
              Skills         = Array.replicatef 6 (fun () -> reader.ReadUInt16 ())
              Resists        = readResists reader
              Unknownx60     = reader.ReadByte ()
              Unknownx61     = reader.ReadByte ()
              Unknownx62     = reader.ReadByte ()
              Unknownx63     = reader.ReadByte ()
              Unknownx64     = reader.ReadByte ()
              Unknownx65     = reader.ReadByte ()
              Zero           = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.EnsureSize 0x68 <| fun () ->
                writer.Write data.ID
                writer.WriteUnknown data.Race
                writer.WriteEnum data.Lightness
                writer.WriteEnum data.Alignment
                writer.Write data.DSourceID
                let baseFlag = Flag.ofSet data.Inherits
                let fixedFlag = if baseFlag <> InheritType.CannotInherit then baseFlag &&& InheritType.CanInherit else baseFlag                
                writer.WriteEnumUInt16 fixedFlag
                writer.Write data.Unknownx0A
                writer.Write data.AttackHitMax
                writer.WriteEnum data.AttackElement
                writer.Write data.Const100x0D
                writer.WriteEnum data.Ailment
                writer.Write data.AilmentChance
                writer.Write data.Unknownx10
                writer.Write data.Strength
                writer.Write data.Vitality
                writer.Write data.Agility
                writer.Write data.Luck
                writer.Write data.Magic
                writer.Write data.Unknownx1F
                if data.Skills.Length > 6 then
                    log LogType.Warning $"Demon {data.ID} has {data.Skills.Length} skills, but max is 6; remaining skills are truncated"
                for idx in 0..(6 - 1) do
                    match Array.tryItem idx data.Skills with
                    | Some skillID -> writer.Write skillID
                    | None         -> writer.Write 0us
                writeResists writer data.Resists
                writer.Write data.Unknownx60
                writer.Write data.Unknownx61
                writer.Write data.Unknownx62
                writer.Write data.Unknownx63
                writer.Write data.Unknownx64
                writer.Write data.Unknownx65
                writer.Write data.Zero

    interface ICSV<DemonInfo> with
        member self.CSVHeader _ _ = refCSVHeader<DemonInfo>
        member self.CSVRows _ data =
            let printResist r = $"{r.Reaction} {r.Multiplier}%%"
            [|[| data.ID.ToString()
                 OrUnknown.simpleToString data.Race
                 data.Level.ToString()
                 data.Lightness.ToString()
                 data.Alignment.ToString()
                 data.DSourceID.ToString()
                 System.String.Join(" ", data.Inherits |> Set.remove InheritType.CanInherit |> Set.map (fun b -> b.ToString()))
                 (int data.Unknownx0A).ToString("X2")
                 data.AttackHitMax.ToString()
                 data.AttackElement.ToString()
                 data.Const100x0D.ToString()
                 data.Ailment.ToString()
                 data.AilmentChance.ToString() + "%"
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx10)
                 data.Strength.ToString()
                 data.Vitality.ToString()
                 data.Agility.ToString()
                 data.Luck.ToString()
                 data.Magic.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx1F)
                 System.String.Join(" ", Array.map (fun b -> b.ToString()) data.Skills)
                 printResist data.Resists.Phys
                 printResist data.Resists.Gun
                 printResist data.Resists.Fire
                 printResist data.Resists.Ice
                 printResist data.Resists.Elec
                 printResist data.Resists.Wind
                 printResist data.Resists.Expel
                 printResist data.Resists.Curse
                 printResist data.Resists.Sleep    
                 printResist data.Resists.Poison   
                 printResist data.Resists.Paralyze 
                 printResist data.Resists.Charm    
                 printResist data.Resists.Mute     
                 printResist data.Resists.Stone    
                 printResist data.Resists.Fear     
                 printResist data.Resists.Strain   
                 printResist data.Resists.Bomb     
                 printResist data.Resists.Rage     
                 printResist data.Resists.Almighty    
                 (int data.Unknownx60).ToString("X2")
                 (int data.Unknownx61).ToString("X2")
                 (int data.Unknownx62).ToString("X2")
                 (int data.Unknownx63).ToString("X2")
                 (int data.Unknownx64).ToString("X2")
                 (int data.Unknownx65).ToString("X2")
                 data.Zero.ToString() |]|]

[<CSVUnpack>]
type ItemDrop =
    { ItemID: uint16
      Chance: uint16
      Zero:   uint16 }

type DemonInfoTab2 =
    { ID:         uint16  // 0x00
      Level:      uint16  // 0x02
      Unknownx04: byte array
      Unknownx1E: uint16     // 0x1E
      Zerox20:    uint16     // 0x20
      Resists:    Resists    // 0x22
      ItemDrop1:  ItemDrop   // 0x48
      ItemDrop2:  ItemDrop   // 0x4E
      ItemDrop3:  ItemDrop   // 0x54
      Unknownx5A: byte array // 0x5A
      TextureID:  uint16     // 0x6A
      Unknownx6C: uint16     // 0x6C
      Unknownx6E: byte       // 0x6E
      Unknownx6F: byte       // 0x6F
      StrID:      string     // 0x70
      Zerox84:    byte       // 0x84
      Unknownx85: byte       // 0x84
      Zerox86:    uint16     // 0x86
    }

type DemonInfoTab2Storer() =
    interface IStorable<DemonInfoTab2> with
        member self.Read config reader =
            let readItemDrop () = {ItemID = reader.ReadUInt16 (); Chance = reader.ReadUInt16 (); Zero = reader.ReadUInt16 ()}
            { ID             = reader.ReadUInt16 ()
              Level          = reader.ReadUInt16 ()
              Unknownx04     = reader.ReadBytes (0x1E - 0x04)
              Unknownx1E     = reader.ReadUInt16 ()
              Zerox20        = reader.ReadUInt16 ()
              Resists        = readResists reader
              ItemDrop1      = readItemDrop ()
              ItemDrop2      = readItemDrop ()
              ItemDrop3      = readItemDrop ()
              Unknownx5A     = reader.ReadBytes (0x6A - 0x5A)
              TextureID      = reader.ReadUInt16 ()
              Unknownx6C     = reader.ReadUInt16 ()
              Unknownx6E     = reader.ReadByte ()
              Unknownx6F     = reader.ReadByte ()
              StrID          = reader.ReadZeroPaddedString (0x84 - 0x70)
              Zerox84        = reader.ReadByte ()
              Unknownx85     = reader.ReadByte ()
              Zerox86        = reader.ReadUInt16 () }
        member self.Write config data writer =
            writer.Write data.ID
            writer.Write data.Level
            writer.Write data.Unknownx04
            writer.Write data.Unknownx1E
            writer.Write data.Zerox20
            writeResists writer data.Resists
            let writeItemDrop item =
                writer.Write item.ItemID
                writer.Write item.Chance
                writer.Write item.Zero
            writeItemDrop data.ItemDrop1
            writeItemDrop data.ItemDrop2
            writeItemDrop data.ItemDrop3
            writer.Write data.Unknownx5A
            writer.Write data.TextureID
            writer.Write data.Unknownx6C
            writer.Write data.Unknownx6E
            writer.Write data.Unknownx6F
            writer.WriteZeroPaddedString (0x84 - 0x70) data.StrID
            writer.Write data.Zerox84
            writer.Write data.Unknownx85
            writer.Write data.Zerox86

    interface ICSV<DemonInfoTab2> with
        member self.CSVHeader _ _ = refCSVHeader<DemonInfoTab2>
        member self.CSVRows _ data =
            let printResist r = $"{r.Reaction} {r.Multiplier}%%"
            [|[| data.ID.ToString()
                 data.Level.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx04)
                 data.Unknownx1E.ToString()
                 data.Zerox20.ToString()
                 printResist data.Resists.Phys
                 printResist data.Resists.Gun
                 printResist data.Resists.Fire
                 printResist data.Resists.Ice
                 printResist data.Resists.Elec
                 printResist data.Resists.Wind
                 printResist data.Resists.Expel
                 printResist data.Resists.Curse
                 printResist data.Resists.Sleep    
                 printResist data.Resists.Poison   
                 printResist data.Resists.Paralyze 
                 printResist data.Resists.Charm    
                 printResist data.Resists.Mute     
                 printResist data.Resists.Stone    
                 printResist data.Resists.Fear     
                 printResist data.Resists.Strain   
                 printResist data.Resists.Bomb     
                 printResist data.Resists.Rage     
                 printResist data.Resists.Almighty
                 data.ItemDrop1.ItemID.ToString()
                 data.ItemDrop1.Chance.ToString() + "%"
                 data.ItemDrop1.Zero.ToString()
                 data.ItemDrop2.ItemID.ToString()
                 data.ItemDrop2.Chance.ToString() + "%"
                 data.ItemDrop2.Zero.ToString()
                 data.ItemDrop3.ItemID.ToString()
                 data.ItemDrop3.Chance.ToString() + "%"
                 data.ItemDrop3.Zero.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx5A)
                 data.TextureID.ToString()
                 (int data.Unknownx6C).ToString("X2")
                 (int data.Unknownx6E).ToString("X2")
                 (int data.Unknownx6F).ToString("X2")
                 "\"" + data.StrID + "\""
                 data.Zerox84.ToString()
                 (int data.Unknownx85).ToString("X2")
                 data.Zerox86.ToString() |]|]