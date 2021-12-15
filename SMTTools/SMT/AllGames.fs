module SMT.AllGames

open SMT.Formats.MSG
open SMT.Formats.TBB
open SMT.Formats.TBCR
open SMT.Formats.TBL
open SMT.Formats.UAsset
open SMT.Games
open SMT.TypeMap
open SMT.Types
open SMT.Utils

let withDefaultStorers () = TypedMap()
                         |> TypedMap.withVal typedefof<byte array>     (BytesStorer 0 |> objStorable)
                         |> TypedMap.withVal typedefof<MSG>            (MSGStorer() |> objStorable)
                         |> TypedMap.withVal typedefof<TBB>            (TBBStorer() |> objStorable)
                         |> TypedMap.withVal typedefof<TBCR>           (TBCRStorer() |> objStorable)
                         |> TypedMap.withVal typedefof<TBL>            (TBLStorer() |> objStorable)
                         |> TypedMap.withVal typedefof<ExtendedUAsset> (UAssetStorer() |> objStorable)

let smtSJR =
    { defaultGame with
          ID   = "SMTSJR"
          Name = "Shin Megami Tensei: Strange Journey Redux"
          OutOfRangeCharMappings = Map.ofList [ '\u01F8', "NL"
                                                '\u04F8', "COLOR"
                                                '\u59F8', "ITEMSTAT"
                                                '\u5AF8', "ITEMINFO"
                                                '\u6CF8', "TARGET"
                                                '\u6DF8', "ICON" ]
          Sections = Map.ofList [ {File = "Compound.tbb";     TableNum = 0}, "Weapons"
                                  {File = "Compound.tbb";     TableNum = 1}, "Armor"
                                  {File = "Compound.tbb";     TableNum = 2}, "Accessories"
                                  {File = "Compound.tbb";     TableNum = 3}, "Apps"
                                  {File = "Compound.tbb";     TableNum = 4}, "Expendables"
                                  {File = "ItemTable.tbb";    TableNum = 0}, "KeyItems"
                                  {File = "ItemTable.tbb";    TableNum = 2}, "Weapons"
                                  {File = "ItemTable.tbb";    TableNum = 3}, "Apps"
                                  {File = "ItemTable.tbb";    TableNum = 4}, "Materials"
                                  {File = "ItemTable.tbb";    TableNum = 5}, "DemonFormas"
                                  {File = "NKMBaseTable.tbb"; TableNum = 0}, "Stats"
                                  {File = "NKMBaseTable.tbb"; TableNum = 1}, "Extra"
                                  {File = "SkillData.tbb";    TableNum = 0}, "Active"
                                  {File = "SkillData.tbb";    TableNum = 1}, "Passive" ]
          Storers                = withDefaultStorers ()
                                |> TypedMap.withVal typedefof<SMTSJR.DemonInfo.DemonInfo> (SMTSJR.DemonInfo.DemonInfoStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTSJR.DemonInfo.DemonInfoTab2> (SMTSJR.DemonInfo.DemonInfoTab2Storer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTSJR.SkillData.SkillData> (SMTSJR.SkillData.SkillDataStorer() |> objStorable)
                                |> ImmutableTypedMap.ofMutable
          TableRowConverters     = Map.ofList
                                       [ {File = "NKMBaseTable.tbb"; TableNum = 0}, objStorable <| SMTSJR.DemonInfo.DemonInfoStorer()
                                         {File = "NKMBaseTable.tbb"; TableNum = 1}, objStorable <| SMTSJR.DemonInfo.DemonInfoTab2Storer()
                                         {File = "SkillData.tbb";    TableNum = 0}, objStorable <| SMTSJR.SkillData.SkillDataStorer() ]
          CSVConverters          = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.CSVConverters
                                       [ typedefof<SMT.Formats.MSG.MSG>,   (storableCSV <| SMT.Formats.MSG.MSGStorer())
                                         typedefof<SMT.Formats.TBL.TBL>,   (storableCSV <| SMT.Formats.TBL.TBLStorer())
                                         typedefof<SMTSJR.DemonInfo.DemonInfo>,     (storableCSV <| SMTSJR.DemonInfo.DemonInfoStorer()) 
                                         typedefof<SMTSJR.DemonInfo.DemonInfoTab2>, (storableCSV <| SMTSJR.DemonInfo.DemonInfoTab2Storer()) 
                                         typedefof<SMTSJR.SkillData.SkillData>,     (storableCSV <| SMTSJR.SkillData.SkillDataStorer()) ]
          ManyCSVConverters      = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.ManyCSVConverters
                                       [ typedefof<SMT.Formats.TBB.TBB>, (storableManyCSV <| SMT.Formats.TBB.TBBStorer()) ] }
                                       
let smtIV =
    { defaultGame with
          ID   = "SMTIV"
          Name = "Shin Megami Tensei IV"
          Storers                = withDefaultStorers ()
                                |> TypedMap.withVal typedefof<SMTIV.DemonSortIndex.RaceName>  (SMTIV.DemonSortIndex.RaceNameStorer()  |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.DemonSortIndex.ActorName> (SMTIV.DemonSortIndex.ActorNameStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.KeyItem>        (SMTIV.ItemTable.KeyItemStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.ConsumableItem> (SMTIV.ItemTable.ConsumableItemStorer()|> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.EquipmentItem>  (SMTIV.ItemTable.EquipmentItemStorer()|> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.RelicCategory>  (SMTIV.ItemTable.RelicCategoryStorer()|> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.RelicItem>      (SMTIV.ItemTable.RelicItemStorer()|> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.LegionSkill>    (SMTIV.ItemTable.LegionSkillStorer()|> objStorable)
                                |> ImmutableTypedMap.ofMutable
          Sections               = Map.ofList
                                       [ {File = "NKMSortIndex.tbb"; TableNum = 0}, "RaceName"
                                         {File = "NKMSortIndex.tbb"; TableNum = 1}, "ActorName"
                                         {File = "ItemTable.tbb";    TableNum = 0}, "KeyItem"
                                         {File = "ItemTable.tbb";    TableNum = 1}, "ConsumableItem"
                                         {File = "ItemTable.tbb";    TableNum = 2}, "EquipmentItem"
                                         {File = "ItemTable.tbb";    TableNum = 3}, "RelicCategory"
                                         {File = "ItemTable.tbb";    TableNum = 4}, "RelicItem"
                                         {File = "ItemTable.tbb";    TableNum = 5}, "LegionSkillStorer" ]
          TableRowConverters     = Map.ofList
                                       [ {File = "NKMSortIndex.tbb"; TableNum = 0}, objStorable <| SMTIV.DemonSortIndex.RaceNameStorer()
                                         {File = "NKMSortIndex.tbb"; TableNum = 1}, objStorable <| SMTIV.DemonSortIndex.ActorNameStorer()
                                         {File = "ItemTable.tbb";    TableNum = 0}, objStorable <| (SMTIV.ItemTable.KeyItemStorer())
                                         {File = "ItemTable.tbb";    TableNum = 1}, objStorable <| SMTIV.ItemTable.ConsumableItemStorer()
                                         {File = "ItemTable.tbb";    TableNum = 2}, objStorable <| SMTIV.ItemTable.EquipmentItemStorer()
                                         {File = "ItemTable.tbb";    TableNum = 3}, objStorable <| SMTIV.ItemTable.RelicCategoryStorer()
                                         {File = "ItemTable.tbb";    TableNum = 4}, objStorable <| SMTIV.ItemTable.RelicItemStorer()
                                         {File = "ItemTable.tbb";    TableNum = 5}, objStorable <| SMTIV.ItemTable.LegionSkillStorer() ]
          CSVConverters          = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.CSVConverters
                                       [ typedefof<SMT.Formats.MSG.MSG>,   (storableCSV <| SMT.Formats.MSG.MSGStorer())
                                         typedefof<SMT.Formats.TBL.TBL>,   (storableCSV <| SMT.Formats.TBL.TBLStorer())
                                         typedefof<SMTIV.DemonSortIndex.RaceName>,   (storableCSV <| SMTIV.DemonSortIndex.RaceNameStorer())
                                         typedefof<SMTIV.DemonSortIndex.ActorName>,  (storableCSV <| SMTIV.DemonSortIndex.ActorNameStorer())
                                         typedefof<SMTIV.ItemTable.KeyItem>,         (storableCSV <| (SMTIV.ItemTable.KeyItemStorer()))
                                         typedefof<SMTIV.ItemTable.ConsumableItem>,  (storableCSV <| SMTIV.ItemTable.ConsumableItemStorer())
                                         typedefof<SMTIV.ItemTable.EquipmentItem>,   (storableCSV <| SMTIV.ItemTable.EquipmentItemStorer())
                                         typedefof<SMTIV.ItemTable.RelicCategory>,   (storableCSV <| SMTIV.ItemTable.RelicCategoryStorer())
                                         typedefof<SMTIV.ItemTable.RelicItem>,       (storableCSV <| SMTIV.ItemTable.RelicItemStorer())
                                         typedefof<SMTIV.ItemTable.LegionSkill>,     (storableCSV <| SMTIV.ItemTable.LegionSkillStorer())
                                        ]
          ManyCSVConverters      = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.ManyCSVConverters
                                       [ typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer()) ]
          OutOfRangeCharMappings = Map.ofList ['\u01F8', "NL"]
          AllowNullCharsInMBM = true}

let smtIVA =
    { defaultGame with
          ID   = "SMTIVA"
          Name = "Shin Megami Tensei IV: Apocalypse"
          Storers                = withDefaultStorers ()
                                |> TypedMap.withVal typedefof<SMTIVA.DemonInfo.DemonInfo>     (SMTIVA.DemonInfo.DemonInfoStorer()       |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIVA.DemonInfo.DemonInfoTab2> (SMTIVA.DemonInfo.DemonInfoTab2Storer()   |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIVA.DemonSortIndex.RaceName>  (SMTIVA.DemonSortIndex.RaceNameStorer()  |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIVA.DemonSortIndex.ActorName> (SMTIVA.DemonSortIndex.ActorNameStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIVA.SkillData.SkillData>      (SMTIVA.SkillData.SkillDataStorer()      |> objStorable)
                                |> ImmutableTypedMap.ofMutable
          TableRowConverters     = Map.ofList
                                       [ {File = "NKMBaseTable.tbb"; TableNum = 0}, objStorable <| SMTIVA.DemonInfo.DemonInfoStorer()
                                         {File = "NKMBaseTable.tbb"; TableNum = 1}, objStorable <| SMTIVA.DemonInfo.DemonInfoTab2Storer()
                                         {File = "NKMSortIndex.tbb"; TableNum = 0}, objStorable <| SMTIVA.DemonSortIndex.RaceNameStorer()
                                         {File = "NKMSortIndex.tbb"; TableNum = 1}, objStorable <| SMTIVA.DemonSortIndex.ActorNameStorer()
                                         {File = "SkillData.tbb";    TableNum = 0}, objStorable <| SMTIVA.SkillData.SkillDataStorer() ]
          CSVConverters          = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.CSVConverters
                                       [ typedefof<SMT.Formats.MSG.MSG>,   (storableCSV <| SMT.Formats.MSG.MSGStorer())
                                         typedefof<SMT.Formats.TBL.TBL>,   (storableCSV <| SMT.Formats.TBL.TBLStorer())
                                         typedefof<SMTIVA.DemonInfo.DemonInfo>,       (storableCSV <| SMTIVA.DemonInfo.DemonInfoStorer())
                                         typedefof<SMTIVA.DemonInfo.DemonInfoTab2>,   (storableCSV <| SMTIVA.DemonInfo.DemonInfoTab2Storer())
                                         typedefof<SMTIVA.DemonSortIndex.RaceName>,   (storableCSV <| SMTIVA.DemonSortIndex.RaceNameStorer())
                                         typedefof<SMTIVA.DemonSortIndex.ActorName>,  (storableCSV <| SMTIVA.DemonSortIndex.ActorNameStorer())
                                         typedefof<SMTIVA.SkillData.SkillData>,       (storableCSV <| SMTIVA.SkillData.SkillDataStorer()) ]
          ManyCSVConverters      = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.ManyCSVConverters
                                       [ typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer()) ]
          OutOfRangeCharMappings = Map.ofList ['\u01F8', "NL"]
          AllowNullCharsInMBM = true}

let smtV =
    { defaultGame with
          ID   = "SMTV"
          Name = "Shin Megami Tensei V"
          Storers                = withDefaultStorers ()
                                |> TypedMap.withVal typedefof<SMTV.SkillData.SkillData>   (SMTV.SkillData.SkillDataStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTV.DemonInfo.DemonInfo>   (SMTV.DemonInfo.DemonInfoStorer() |> objStorable)
                                |> ImmutableTypedMap.ofMutable
          TableRowConverters     = Map.ofList
                [ {File = "SkillData.uasset";    TableNum = 0}, objStorable <| SMTV.SkillData.SkillDataStorer()
                  {File = "NKMBaseTable.uasset"; TableNum = 0}, objStorable <| SMTV.DemonInfo.DemonInfoStorer() ]
          CSVConverters          = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.CSVConverters
                                       [ typedefof<byte array>,               (storableCSV <| BytesStorer 0)
                                         typedefof<SMT.Formats.MSG.MSG>,      (storableCSV <| SMT.Formats.MSG.MSGStorer())
                                         typedefof<SMT.Formats.TBL.TBL>,      (storableCSV <| SMT.Formats.TBL.TBLStorer())
                                         typedefof<SMTV.DemonInfo.DemonInfo>, (storableCSV <| SMTV.DemonInfo.DemonInfoStorer())
                                         typedefof<SMTV.SkillData.SkillData>, (storableCSV <| SMTV.SkillData.SkillDataStorer()) ]
          ManyCSVConverters      = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.ManyCSVConverters
                                       [ typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer()) ]}

                                       
let unknown =
    { defaultGame with
          ID   = "Unknown"
          Name = "Unknown"
          Storers                = withDefaultStorers ()
                                |> ImmutableTypedMap.ofMutable
          CSVConverters          = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.CSVConverters
                                       [ typedefof<byte array>,               (storableCSV <| BytesStorer 0)
                                         typedefof<SMT.Formats.MSG.MSG>,   (storableCSV <| SMT.Formats.MSG.MSGStorer())
                                         typedefof<SMT.Formats.TBL.TBL>,   (storableCSV <| SMT.Formats.TBL.TBLStorer()) ]
          ManyCSVConverters      = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.ManyCSVConverters
                                       [ typedefof<SMT.Formats.TBB.TBB>,   (storableManyCSV <| SMT.Formats.TBB.TBBStorer())
                                         typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer()) ] }

let GamesByID = Map.ofList [ smtSJR.ID.ToUpper(),  smtSJR
                             smtIV.ID.ToUpper(),   smtIV
                             smtIVA.ID.ToUpper(),  smtIVA
                             smtV.ID.ToUpper(),    smtV
                             unknown.ID.ToUpper(), unknown]

let genericStorableFormats =
    [ storableFormat <| SMT.Formats.TBB.TBBStorer()
      storableFormat <| SMT.Formats.TBCR.TBCRStorer()
      storableFormat <| SMT.Formats.MSG.MSGStorer()
      storableFormat <| SMT.Formats.UAsset.UAssetStorer() ] 