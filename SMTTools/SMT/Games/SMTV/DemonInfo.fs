module SMT.Games.SMTV.DemonInfo

open SMT.Types
open SMT.Utils

type Race =
    | Dummy        = 0x00uy
    | GodlyUnused  = 0x01uy
    | Herald       = 0x02uy
    | Megami       = 0x03uy
    | Avian        = 0x04uy
    | Divine       = 0x05uy
    | Yoma         = 0x06uy
    | Vile         = 0x07uy
    | Raptor       = 0x08uy
    | UnusedReiken = 0x09uy
    | Deity        = 0x0Auy
    | Wargod       = 0x0Buy
    | Avatar       = 0x0Cuy
    | Holy         = 0x0Duy
    | Genma        = 0x0Euy
    | Element      = 0x0Fuy
    | Mitama       = 0x10uy
    | Fairy        = 0x11uy
    | Beast        = 0x12uy
    | Jirae        = 0x13uy
    | Fiend        = 0x14uy
    | Jaki         = 0x15uy
    | Wilder       = 0x16uy
    | Fury         = 0x17uy
    | Lady         = 0x18uy
    | Dragon       = 0x19uy
    | Kishin       = 0x1Auy
    | Kunitsu      = 0x1Buy
    | Femme        = 0x1Cuy
    | Brute        = 0x1Duy
    | Fallen       = 0x1Euy
    | Night        = 0x1Fuy
    | Snake        = 0x20uy
    | Tyran        = 0x21uy
    | Drake        = 0x22uy
    | Haunt        = 0x23uy
    | Foul         = 0x24uy
    | King         = 0x25uy
    | UnusedDaimou = 0x26uy
    | Meta         = 0x27uy
    | Nahabino     = 0x28uy
    | ProtoFiend   = 0x29uy
    | Matter       = 0x2Auy
    | Panagia      = 0x2Buy

type Resist =
    | Drain // 1000
    | Repel // 999
    | MultBy of int

[<CSVUnpack("Affinity")>]
type Affinities =
    { Phys:     int32      // 0x184
      Fire:     int32      // 0x188
      Ice:      int32      // 0x18C
      Elec:     int32      // 0x190
      Force:    int32      // 0x194
      Light:    int32      // 0x198
      Dark:     int32      // 0x19C
      Almighty: int32      // 0x1A0
      Ailment:  int32      // 0x1A4
      Support:  int32      // 0x1A8
      Healing:  int32      // 0x1AC
    }

[<CSVUnpack("Resist")>]
type Resists =
    { Phys:        Resist      // 0x114
      Fire:        Resist      // 0x118
      Ice:         Resist      // 0x11C
      Elec:        Resist      // 0x120
      Force:       Resist      // 0x124
      Light:       Resist      // 0x128
      Dark:        Resist      // 0x12C
      Almighty:    Resist      // 0x130
      Poison:      Resist      // 0x134
      Unknownx138: Resist      // 0x138
      Panic:       Resist      // 0x13C
      Charm:       Resist      // 0x140
      Sleep:       Resist      // 0x144
      Seal:        Resist      // 0x148
      Unknownx14C: Resist      // 0x14C
      Unknownx150: Resist      // 0x150
      Unknownx154: Resist      // 0x154
      Unknownx158: Resist      // 0x158
      Unknownx15C: Resist      // 0x15C
      Unknownx160: Resist      // 0x160
      Mirage:      Resist      // 0x164
      Unknownx168: Resist      // 0x168
      Unknownx16C: Resist      // 0x16C
      Unknownx170: Resist      // 0x170
      Unknownx174: Resist      // 0x174
      Unknownx178: Resist      // 0x178
      Unknownx17C: Resist      // 0x17C
      Unknownx180: Resist      // 0x180
    }

type DemonInfo =
    { ID:                uint32            // 0x000
      Unknownx004:       byte array        // 0x004
      Race:              Race              // 0x008
      Unknownx006:       byte array        // 00x06
      Level:             uint16            // 0x014
      Unknownx016:       byte array        // 0x016
      Strength:          int               // 0x040
      Vitality:          int               // 0x044
      Magic:             int               // 0x048
      Agility:           int               // 0x04C
      Luck:              int               // 0x050
      Unknownx054:       byte array        // 0x054
      Resists:           Resists           // 0x114
      Affinities:        Affinities        // 0x184
      Unknownx1B0:       byte array        // 0x1B0
    }

type DemonInfoStorer() =
    interface IStorable<DemonInfo> with
        member self.Read config reader =
            let readResist () =
                match reader.ReadInt32 () with
                | 1000 -> Drain
                | 999  -> Repel
                | e    -> MultBy e
            { ID               = reader.ReadUInt32 ()
              Unknownx004      = reader.ReadBytes <| 0x08 - 0x04
              Race             = reader.ReadByte () |> enumOrUnknown |> OrUnknown.unwrapOrErr
              Unknownx006      = reader.ReadBytes <| 0x14 - 0x09
              Level            = reader.ReadUInt16 ()
              Unknownx016      = reader.ReadBytes <| 0x40 - 0x16
              Strength         = reader.ReadInt32 ()
              Vitality         = reader.ReadInt32 ()
              Magic            = reader.ReadInt32 ()
              Agility          = reader.ReadInt32 ()
              Luck             = reader.ReadInt32 ()
              Unknownx054      = reader.ReadBytes <| 0x114 - 0x54
              Resists =
                  { Phys        = readResist ()
                    Fire        = readResist ()
                    Ice         = readResist ()
                    Elec        = readResist ()
                    Force       = readResist ()
                    Light       = readResist ()
                    Dark        = readResist ()
                    Almighty    = readResist ()
                    Poison      = readResist ()
                    Unknownx138 = readResist ()
                    Panic       = readResist ()
                    Charm       = readResist ()
                    Sleep       = readResist ()
                    Seal        = readResist ()
                    Unknownx14C = readResist ()
                    Unknownx150 = readResist ()
                    Unknownx154 = readResist ()
                    Unknownx158 = readResist ()
                    Unknownx15C = readResist ()
                    Unknownx160 = readResist ()
                    Mirage      = readResist ()
                    Unknownx168 = readResist ()
                    Unknownx16C = readResist ()
                    Unknownx170 = readResist ()
                    Unknownx174 = readResist ()
                    Unknownx178 = readResist ()
                    Unknownx17C = readResist ()
                    Unknownx180 = readResist () }
              Affinities =
                  { Phys     = reader.ReadInt32 ()
                    Fire     = reader.ReadInt32 ()
                    Ice      = reader.ReadInt32 ()
                    Elec     = reader.ReadInt32 ()
                    Force    = reader.ReadInt32 ()
                    Light    = reader.ReadInt32 ()
                    Dark     = reader.ReadInt32 ()
                    Almighty = reader.ReadInt32 ()
                    Ailment  = reader.ReadInt32 ()
                    Support  = reader.ReadInt32 ()
                    Healing  = reader.ReadInt32 () }
              Unknownx1B0      = reader.ReadBytes <| 0x1C4 - 0x1B0
              }

        member self.Write config data writer =
            writer.EnsureSize 0x1C4 <| fun () ->
                let writeResist =
                    function
                    | Drain    -> writer.Write 1000
                    | Repel    -> writer.Write 999
                    | MultBy i -> writer.Write i
                writer.Write data.ID
                writer.Write data.Unknownx004
                writer.WriteEnum data.Race
                writer.Write data.Unknownx006
                writer.Write data.Level
                writer.Write data.Unknownx016
                writer.Write data.Strength
                writer.Write data.Vitality
                writer.Write data.Magic
                writer.Write data.Agility
                writer.Write data.Luck
                writer.Write data.Unknownx054
                writeResist data.Resists.Phys
                writeResist data.Resists.Fire
                writeResist data.Resists.Ice
                writeResist data.Resists.Elec
                writeResist data.Resists.Force
                writeResist data.Resists.Light
                writeResist data.Resists.Dark
                writeResist data.Resists.Almighty
                writeResist data.Resists.Poison
                writeResist data.Resists.Unknownx138
                writeResist data.Resists.Panic
                writeResist data.Resists.Charm
                writeResist data.Resists.Sleep
                writeResist data.Resists.Seal
                writeResist data.Resists.Unknownx14C
                writeResist data.Resists.Unknownx150
                writeResist data.Resists.Unknownx154
                writeResist data.Resists.Unknownx158
                writeResist data.Resists.Unknownx15C
                writeResist data.Resists.Unknownx160
                writeResist data.Resists.Mirage
                writeResist data.Resists.Unknownx168
                writeResist data.Resists.Unknownx16C
                writeResist data.Resists.Unknownx170
                writeResist data.Resists.Unknownx174
                writeResist data.Resists.Unknownx178
                writeResist data.Resists.Unknownx17C
                writeResist data.Resists.Unknownx180
                writer.Write data.Affinities.Phys
                writer.Write data.Affinities.Fire
                writer.Write data.Affinities.Ice
                writer.Write data.Affinities.Elec
                writer.Write data.Affinities.Force
                writer.Write data.Affinities.Light
                writer.Write data.Affinities.Dark
                writer.Write data.Affinities.Almighty
                writer.Write data.Affinities.Ailment
                writer.Write data.Affinities.Support
                writer.Write data.Affinities.Healing
                writer.Write data.Unknownx1B0

    interface ICSV<DemonInfo> with
        member self.CSVHeader _ _ = refCSVHeader<DemonInfo>
        member self.CSVRows _ data =
            [|[| data.ID.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx004) 
                 data.Race.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx006) 
                 data.Level.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx016)
                 data.Strength.ToString()
                 data.Vitality.ToString()
                 data.Magic.ToString()
                 data.Agility.ToString()
                 data.Luck.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx054)
                 data.Resists.Phys.ToString()
                 data.Resists.Fire.ToString()
                 data.Resists.Ice.ToString()
                 data.Resists.Elec.ToString()
                 data.Resists.Force.ToString()
                 data.Resists.Light.ToString()
                 data.Resists.Dark.ToString()
                 data.Resists.Almighty.ToString()
                 data.Resists.Poison.ToString()
                 data.Resists.Unknownx138.ToString()
                 data.Resists.Panic.ToString()
                 data.Resists.Charm.ToString()
                 data.Resists.Sleep.ToString()
                 data.Resists.Seal.ToString()
                 data.Resists.Unknownx14C.ToString()
                 data.Resists.Unknownx150.ToString()
                 data.Resists.Unknownx154.ToString()
                 data.Resists.Unknownx158.ToString()
                 data.Resists.Unknownx15C.ToString()
                 data.Resists.Unknownx160.ToString()
                 data.Resists.Mirage.ToString()
                 data.Resists.Unknownx168.ToString()
                 data.Resists.Unknownx16C.ToString()
                 data.Resists.Unknownx170.ToString()
                 data.Resists.Unknownx174.ToString()
                 data.Resists.Unknownx178.ToString()
                 data.Resists.Unknownx17C.ToString()
                 data.Resists.Unknownx180.ToString()
                 data.Affinities.Phys.ToString()
                 data.Affinities.Fire.ToString()
                 data.Affinities.Ice.ToString()
                 data.Affinities.Elec.ToString()
                 data.Affinities.Force.ToString()
                 data.Affinities.Light.ToString()
                 data.Affinities.Dark.ToString()
                 data.Affinities.Almighty.ToString()
                 data.Affinities.Ailment.ToString()
                 data.Affinities.Support.ToString()
                 data.Affinities.Healing.ToString()
                 System.String.Join(" ", Array.map (fun b -> (int b).ToString("X2")) data.Unknownx1B0) 
            |]|]