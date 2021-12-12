module SMT.Games.SMTIVA.DemonInfo

open SMT.Formats.TBCR
open SMT.Formats.TBL
open SMT.Types
open SMT.Utils

let bytesToString bytes = System.String.Join(" ", Array.map (fun b -> "0x" + (int b).ToString("X2")) bytes)

let getSameFileTBLEntries<'a> config key =
    match config.Context.SameFileValues.TryGet<TBL> key with
    | Some tbl ->
        if tbl.Entries.Length > 0 && tbl.Entries.[0] :? 'a
        then Some tbl
        else None
    | None   -> None

let findSameFileTBLEntry<'a> pred config key =
    match getSameFileTBLEntries config key with
    | None     -> None
    | Some tbb ->
        let mutable idx = 0
        let mutable retVal = None
        while idx < tbb.Entries.Length do
            match tbb.Entries.[idx] with
            | :? 'a as a when pred a ->
                idx    <- tbb.Entries.Length
                retVal <- Some a
            | _ -> idx <- idx + 1
        retVal

// Note: doesn't seem to affect fusion on its own? Could still fusion Mamedanuki + Cyber Centaur to get normal result Sandman
type Race = QMark = 0x00u | Herald = 0x01u | Megami = 0x02u | Avian = 0x03u | Tree = 0x04u | Divine = 0x05u | Flight = 0x06u | Yoma = 0x07u | Nymph = 0x08u | Vile = 0x09u | Raptor = 0x0Au
          | Wood = 0x0Bu | Deity = 0x0Cu | Avatar = 0x0Du | Holy = 0x0Eu | Genma = 0x0Fu | Fairy = 0x10u | Beast = 0x11u | Jirae = 0x12u | Snake = 0x13u
          | Reaper = 0x14u | Wilder = 0x15u | Jaki = 0x16u | Vermin = 0x17u | Fury = 0x18u | Lady = 0x19u | Dragon = 0x1Au | Kishin = 0x1Bu | Fallen = 0x1Cu
          | Brute = 0x1Du | Femme = 0x1Eu | Night = 0x1Fu | Tyrant = 0x20u | Drake = 0x21u | Spirit = 0x22u | Foul = 0x23u | Ghost = 0x24u | Mitama = 0x25u
          | Elemental = 0x26u | Fiend = 0x27u | Enigma = 0x28u | Food = 0x29u | Zealot = 0x2Au | Archaic = 0x2Bu
          | Cyber = 0x2Cu | King = 0x2Du | TripleQMark = 0x2Eu | Godly = 0x2Fu | Entity = 0x30u | Amatsu = 0x31u | Kunitsu = 0x32u | Famed = 0x33u | Human = 0x34u
          | Chaos = 0x35u | Undead = 0x36u | Horde = 0x37u
          | HumanGhost = 0x38u | Samurai = 0x39u | Hybric = 0x3Au
          | Primal = 0x3Bu | Awakened = 0x3Cu | PostHuman = 0x3Du | Astral = 0x3Eu | Godslayer = 0x3Fu | Replicant = 0x40u | CyberDupe = 0x44u

type ReactionType = Normal = 0x00uy | Null = 0x04uy | Weak = 0x08uy | AilmentWeak = 0x09uy | Drain = 0x10uy | Repel = 0x12uy | Resist = 0x14uy

type ElementalReaction =
    { Multiplier: byte
      Reaction:   OrUnknown<ReactionType, byte> }

type DemonInfo =
    { ID:                  uint16
      Race:                OrUnknown<Race, uint32> // 0x02
      Level:               uint16     // 0x06
      Unknownx08:          byte array // 0x08
      Strength:            uint16     // 0x1E
      Dexterity:           uint16     // 0x20
      Magic:               uint16     // 0x22
      Agility:             uint16     // 0x24
      Luck:                uint16     // 0x26
      Unknownx28:          byte array // 0x28
      BaseSkills:          uint16 array        // 0x40 - 0x50
      LearnableSkills:     Map<uint16, uint16> // 0x50 - 0x70
      PhysReaction:        ElementalReaction   // 0x70
      GunReaction:         ElementalReaction   // 0x72
      FireReaction:        ElementalReaction   // 0x74
      IceReaction:         ElementalReaction   // 0x76
      ElecReaction:        ElementalReaction   // 0x78
      ForceReaction:       ElementalReaction   // 0x7A
      LightReaction:       ElementalReaction   // 0x7C
      DarkReaction:        ElementalReaction   // 0x7E
      UnknownReactionx80:  ElementalReaction   // 0x80
      UnknownReactionx82:  ElementalReaction   // 0x82
      UnknownReactionx84:  ElementalReaction   // 0x84
      PoisonReaction:      ElementalReaction   // 0x86
      PanicReaction:       ElementalReaction   // 0x88
      SleepReaction:       ElementalReaction   // 0x8A
      BindReaction:        ElementalReaction   // 0x8C
      SickReaction:        ElementalReaction   // 0x8E
      UnknownReactionx90:  ElementalReaction   // 0x90
      CharmReaction:       ElementalReaction   // 0x92
      DazeReaction:        ElementalReaction   // 0x94
      MuteReaction:        ElementalReaction   // 0x96
      UnknownReactionx96:  ElementalReaction   // 0x96
      Unknownx9A:          byte array // 0x9A
      AffinityPhysical:    sbyte      // 0xA1 & 0xA2 duplicated
      AffinityGun:         sbyte      // 0xA3
      AffinityFire:        sbyte      // 0xA4
      AffinityIce:         sbyte      // 0xA5
      AffinityElec:        sbyte      // 0xA6
      AffinityForce:       sbyte      // 0xA7
      AffinityLight:       sbyte      // 0xA8
      AffinityDark:        sbyte      // 0xA9
      AffinityAlmighty:    sbyte      // 0xAA & 0xAB duplicated
      AffinityAilment:     sbyte      // 0xAC & 0xAD duplicated
      AffinityHeal:        sbyte      // 0xAE & 0xAF duplicated
      AffinitySupport:     sbyte      // 0xB0 & 0xB1 duplicated
      ZeroxB2:             byte       // 0xB2
      UnknownxB3:          byte       // 0xB3
      } // 0xB4 length

and DemonInfoStorer() =
    interface IStorable<DemonInfo> with
        member self.Read config reader =
            let readReaction () = {Multiplier = reader.ReadByte (); Reaction = reader.ReadByte () |> enumOrUnknown}
            let id           = reader.ReadUInt16 ()
            let race         = reader.ReadUInt32 () |> enumOrUnknown
            let level        = reader.ReadUInt16 ()
            let warnPairMismatch s a b =
                if a <> b then // these are duplicated for some reason; not sure if behavior is changed, but in SMTIVA they always match, so we want to ensure this
                    log Warning $"Demon {id}'s {s} pair didn't match ({a} <> {b})"
                a
            { ID                = id
              Race              = race
              Level             = level
              Unknownx08        = reader.ReadBytes <| 0x1E - 0x08
              Strength          = reader.ReadUInt16 ()
              Dexterity         = reader.ReadUInt16 ()
              Magic             = reader.ReadUInt16 ()
              Agility           = reader.ReadUInt16 ()
              Luck              = reader.ReadUInt16 ()
              Unknownx28        = reader.ReadBytes <| 0x40 - 0x28
              BaseSkills        = Array.replicatef ((0x50 - 0x40) / sizeof<uint16>) reader.ReadUInt16 |> Array.filter ((<>) 0us)
              LearnableSkills   = Array.replicatef ((0x70 - 0x50) / (sizeof<uint16> * 2)) (fun () -> reader.ReadUInt16 (), reader.ReadUInt16 ()) |> Map.ofArray |> Map.remove 0us
              PhysReaction       = readReaction ()
              GunReaction        = readReaction ()
              FireReaction       = readReaction ()
              IceReaction        = readReaction ()
              ElecReaction       = readReaction ()
              ForceReaction      = readReaction ()
              LightReaction      = readReaction ()
              DarkReaction       = readReaction ()
              UnknownReactionx80 = readReaction ()
              UnknownReactionx82 = readReaction ()
              UnknownReactionx84 = readReaction ()
              PoisonReaction     = readReaction ()
              PanicReaction      = readReaction ()
              SleepReaction      = readReaction ()
              BindReaction       = readReaction ()
              SickReaction       = readReaction ()
              UnknownReactionx90 = readReaction ()
              CharmReaction      = readReaction ()
              DazeReaction       = readReaction ()
              MuteReaction       = readReaction ()
              UnknownReactionx96 = readReaction ()
              Unknownx9A        = reader.ReadBytes (0xA1 - 0x9A)
              AffinityPhysical  = warnPairMismatch "AffinityPhysical" (reader.ReadSByte ()) (reader.ReadSByte ())
              AffinityGun       = reader.ReadSByte ()
              AffinityFire      = reader.ReadSByte ()
              AffinityIce       = reader.ReadSByte ()
              AffinityElec      = reader.ReadSByte ()
              AffinityForce     = reader.ReadSByte ()
              AffinityLight     = reader.ReadSByte ()
              AffinityDark      = reader.ReadSByte ()
              AffinityAlmighty  = warnPairMismatch "AffinityAlmighty" (reader.ReadSByte ()) (reader.ReadSByte ())
              AffinityAilment   = warnPairMismatch "AffinityAilment" (reader.ReadSByte ()) (reader.ReadSByte ())
              AffinityHeal      = warnPairMismatch "AffinityHeal" (reader.ReadSByte ()) (reader.ReadSByte ())
              AffinitySupport   = warnPairMismatch "Affinitysupport" (reader.ReadSByte ()) (reader.ReadSByte ())
              ZeroxB2           = reader.ReadByte ()
              UnknownxB3        = reader.ReadByte () }
        member self.Write config data writer =
            failwith "Unimplemented"

    interface ICSV<DemonInfo> with
        member self.CSVHeader config _ = 
            match getSameFileTBLEntries<DemonInfoTab2> config 1 with
            | Some _ -> Array.append [|"StrID"|] <| refCSVHeader<DemonInfo>
            | None   -> refCSVHeader<DemonInfo>
        member self.CSVRows config data =
            let firstColumn =
                match findSameFileTBLEntry<DemonInfoTab2> (fun t -> t.ID = data.ID) config 1 with
                | None     -> [||]
                | Some tab -> [| "\"" + String.sanitizeControlChars tab.StrID + "\"" |]
            let printReaction r = OrUnknown.simpleToString r.Reaction + $" {r.Multiplier}%%"
            [| Array.append firstColumn
              [| data.ID.ToString()
                 OrUnknown.simpleToString data.Race
                 data.Level.ToString()
                 bytesToString data.Unknownx08
                 data.Strength.ToString()
                 data.Dexterity.ToString()
                 data.Magic.ToString()
                 data.Agility.ToString()
                 data.Luck.ToString()
                 bytesToString data.Unknownx28
                 Array.csvToString data.BaseSkills
                 Map.csvToString data.LearnableSkills
                 printReaction data.PhysReaction
                 printReaction data.GunReaction
                 printReaction data.FireReaction
                 printReaction data.IceReaction
                 printReaction data.ElecReaction
                 printReaction data.ForceReaction
                 printReaction data.LightReaction
                 printReaction data.DarkReaction
                 printReaction data.UnknownReactionx80
                 printReaction data.UnknownReactionx82
                 printReaction data.UnknownReactionx84
                 printReaction data.PoisonReaction
                 printReaction data.PanicReaction
                 printReaction data.SleepReaction
                 printReaction data.BindReaction
                 printReaction data.SickReaction
                 printReaction data.UnknownReactionx90
                 printReaction data.CharmReaction
                 printReaction data.DazeReaction
                 printReaction data.MuteReaction
                 printReaction data.UnknownReactionx96
                 bytesToString data.Unknownx9A
                 data.AffinityPhysical.ToString ()
                 data.AffinityGun.ToString ()
                 data.AffinityFire.ToString ()
                 data.AffinityIce.ToString ()
                 data.AffinityElec.ToString ()
                 data.AffinityForce.ToString ()
                 data.AffinityLight.ToString ()
                 data.AffinityDark.ToString ()
                 data.AffinityAlmighty.ToString ()
                 data.AffinityAilment.ToString ()
                 data.AffinityHeal.ToString ()
                 data.AffinitySupport.ToString ()
                 "x" + (int data.ZeroxB2).ToString "X2"
                 data.UnknownxB3.ToString () |]|]

and DemonInfoTab2 = // placeholder until purpose determined
    { ID:         uint16     // 0x00
      Level:      byte       // 0x02
      Unknownx03: byte array // 0x03
      PortraitID: uint16     // 0x8C
      StrID:      string     // 0x8E through 0xA2 20 bytes
      UnknownxA2: uint16     // 0xA4
      UnknownxA4: uint16     // 0xA4
      ZeroxA6:    uint16     // 0xA6
      }

and DemonInfoTab2Storer() =
    let bytesToString bytes = System.String.Join(" ", Array.map (fun b -> "0x" + (int b).ToString("X2")) bytes)
    interface IStorable<DemonInfoTab2> with
        member self.Read config reader =
            let id           = reader.ReadUInt16 ()
            { ID         = id
              Level      = reader.ReadByte ()
              Unknownx03 = reader.ReadBytes (0x8C - 0x03)
              PortraitID = reader.ReadUInt16 ()
              StrID      = reader.ReadZeroPaddedString 20
              UnknownxA2 = reader.ReadUInt16 ()
              UnknownxA4 = reader.ReadUInt16 ()
              ZeroxA6    = reader.ReadUInt16 () }
        member self.Write config data writer =
            failwith "Unimplemented"

    interface ICSV<DemonInfoTab2> with
        member self.CSVHeader _ _ = refCSVHeader<DemonInfoTab2>
        member self.CSVRows _ data =
            [|[| data.ID.ToString()
                 data.Level.ToString()
                 bytesToString data.Unknownx03
                 data.PortraitID.ToString()
                 "\"" + String.sanitizeControlChars data.StrID + "\""
                 "x" + (int data.UnknownxA4).ToString "X2"
                 "x" + (int data.ZeroxA6).ToString "X2" |]|]