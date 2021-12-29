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
                                |> TypedMap.withVal typedefof<SMTIV.Bible.CompendiumUIMessage> (SMTIV.Bible.CompendiumUIMessageStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.DemonSortIndex.RaceName>   (SMTIV.DemonSortIndex.RaceNameStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.DemonSortIndex.ActorName>  (SMTIV.DemonSortIndex.ActorNameStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.KeyItem>         (SMTIV.ItemTable.KeyItemStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.ConsumableItem>  (SMTIV.ItemTable.ConsumableItemStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.EquipmentItem>   (SMTIV.ItemTable.EquipmentItemStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.RelicCategory>   (SMTIV.ItemTable.RelicCategoryStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.RelicItem>       (SMTIV.ItemTable.RelicItemStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.ItemTable.LegionSkill>     (SMTIV.ItemTable.LegionSkillStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.NPCRoom.NPCRoomInfo1>      (SMTIV.NPCRoom.NPCRoomInfo1Storer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.NPCRoom.NPCRoomInfo2>      (SMTIV.NPCRoom.NPCRoomInfo2Storer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.NPCHaichi>           (SMTIV.Quest.NPCHaichiStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.QCMessage>           (SMTIV.Quest.QCMessageStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.QuestData>           (SMTIV.Quest.QuestDataStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.QuestLocation>       (SMTIV.Quest.QuestLocationStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.QuestReward>         (SMTIV.Quest.QuestRewardStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.HunterRanking>       (SMTIV.Quest.HunterRankingStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Quest.RankingCategory>     (SMTIV.Quest.RankingCategoryStorer() |> objStorable)
                                |> TypedMap.withVal typedefof<SMTIV.Skill.SkillData>           (SMTIV.Skill.SkillDataStorer() |> objStorable)
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
                                       [ {File = "BibleCalcTable.tbb"; TableNum = 4}, objStorable <| SMTIV.Bible.CompendiumUIMessageStorer()
                                         {File = "NKMSortIndex.tbb";   TableNum = 0}, objStorable <| SMTIV.DemonSortIndex.RaceNameStorer()
                                         {File = "NKMSortIndex.tbb";   TableNum = 1}, objStorable <| SMTIV.DemonSortIndex.ActorNameStorer()
                                         {File = "ItemTable.tbb";      TableNum = 0}, objStorable <| (SMTIV.ItemTable.KeyItemStorer())
                                         {File = "ItemTable.tbb";      TableNum = 1}, objStorable <| SMTIV.ItemTable.ConsumableItemStorer()
                                         {File = "ItemTable.tbb";      TableNum = 2}, objStorable <| SMTIV.ItemTable.EquipmentItemStorer()
                                         {File = "ItemTable.tbb";      TableNum = 3}, objStorable <| SMTIV.ItemTable.RelicCategoryStorer()
                                         {File = "ItemTable.tbb";      TableNum = 4}, objStorable <| SMTIV.ItemTable.RelicItemStorer()
                                         {File = "ItemTable.tbb";      TableNum = 5}, objStorable <| SMTIV.ItemTable.LegionSkillStorer()
                                         {File = "NpcRoomTable.tbb";   TableNum = 0}, objStorable <| SMTIV.NPCRoom.NPCRoomInfo1Storer()
                                         {File = "NpcRoomTable.tbb";   TableNum = 1}, objStorable <| SMTIV.NPCRoom.NPCRoomInfo2Storer()
                                         {File = "NpcHaichiTable.tbb"; TableNum = 0}, objStorable <| SMTIV.Quest.NPCHaichiStorer()
                                         {File = "QcName.tbb";         TableNum = 0}, objStorable <| SMTIV.Quest.QCMessageStorer()
                                         {File = "QcName.tbb";         TableNum = 1}, objStorable <| SMTIV.Quest.QCMessageStorer()
                                         {File = "QuestData.tbb";      TableNum = 0}, objStorable <| SMTIV.Quest.QuestDataStorer()
                                         {File = "RankingTable.tbb";   TableNum = 0}, objStorable <| SMTIV.Quest.RankingCategoryStorer()
                                         {File = "RankingTable.tbb";   TableNum = 1}, objStorable <| SMTIV.Quest.HunterRankingStorer()
                                         {File = "RankingTable.tbb";   TableNum = 2}, objStorable <| SMTIV.Quest.RankingCategoryStorer()
                                         {File = "RankingTable.tbb";   TableNum = 3}, objStorable <| SMTIV.Quest.HunterRankingStorer()
                                         {File = "SkillData.tbb";      TableNum = 0}, objStorable <| SMTIV.Skill.SkillDataStorer() 
                                         {File = "SubQuestData.tbb";   TableNum = 0}, objStorable <| SMTIV.Quest.QuestDataStorer()
                                         {File = "SubQuestData.tbb";   TableNum = 1}, objStorable <| SMTIV.Quest.QuestRewardStorer() ]
          CSVConverters          = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.CSVConverters
                                       [ typedefof<SMT.Formats.MSG.MSG>,   (storableCSV <| SMT.Formats.MSG.MSGStorer())
                                         typedefof<SMT.Formats.TBL.TBL>,   (storableCSV <| SMT.Formats.TBL.TBLStorer())
                                         typedefof<SMTIV.Bible.CompendiumUIMessage>, (storableCSV <| SMTIV.Bible.CompendiumUIMessageStorer())
                                         typedefof<SMTIV.DemonSortIndex.RaceName>,   (storableCSV <| SMTIV.DemonSortIndex.RaceNameStorer())
                                         typedefof<SMTIV.DemonSortIndex.ActorName>,  (storableCSV <| SMTIV.DemonSortIndex.ActorNameStorer())
                                         typedefof<SMTIV.ItemTable.KeyItem>,         (storableCSV <| (SMTIV.ItemTable.KeyItemStorer()))
                                         typedefof<SMTIV.ItemTable.ConsumableItem>,  (storableCSV <| SMTIV.ItemTable.ConsumableItemStorer())
                                         typedefof<SMTIV.ItemTable.EquipmentItem>,   (storableCSV <| SMTIV.ItemTable.EquipmentItemStorer())
                                         typedefof<SMTIV.ItemTable.RelicCategory>,   (storableCSV <| SMTIV.ItemTable.RelicCategoryStorer())
                                         typedefof<SMTIV.ItemTable.RelicItem>,       (storableCSV <| SMTIV.ItemTable.RelicItemStorer())
                                         typedefof<SMTIV.ItemTable.LegionSkill>,     (storableCSV <| SMTIV.ItemTable.LegionSkillStorer())
                                         typedefof<SMTIV.NPCRoom.NPCRoomInfo1>,      (storableCSV <| SMTIV.NPCRoom.NPCRoomInfo1Storer())
                                         typedefof<SMTIV.NPCRoom.NPCRoomInfo2>,      (storableCSV <| SMTIV.NPCRoom.NPCRoomInfo2Storer())
                                         typedefof<SMTIV.Quest.NPCHaichi>,           (storableCSV <| SMTIV.Quest.NPCHaichiStorer())
                                         typedefof<SMTIV.Quest.QCMessage>,           (storableCSV <| SMTIV.Quest.QCMessageStorer())
                                       //typedefof<SMTIV.Quest.QuestLocation>,       (storableCSV <| SMTIV.Quest.QuestLocationStorer())
                                         typedefof<SMTIV.Quest.QuestData>,           (storableCSV <| SMTIV.Quest.QuestDataStorer())
                                         typedefof<SMTIV.Quest.QuestReward>,         (storableCSV <| SMTIV.Quest.QuestRewardStorer())
                                         typedefof<SMTIV.Quest.HunterRanking>,       (storableCSV <| SMTIV.Quest.HunterRankingStorer())
                                         typedefof<SMTIV.Quest.RankingCategory>,     (storableCSV <| SMTIV.Quest.RankingCategoryStorer())
                                         typedefof<SMTIV.Skill.SkillData>,           (storableCSV <| SMTIV.Skill.SkillDataStorer())
                                       ]
          ManyCSVConverters      = List.fold (fun conv (t, stor) -> conv.Add t stor) defaultGame.ManyCSVConverters
                                       [ typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer()) ]
          OutOfRangeCharMappings = Map.ofList ['\u01F8', "NL"]
          AllowNullCharsInMBM = true }

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
                                       [ typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer())
                                         typedefof<SMT.Formats.UAsset.ExtendedUAsset>, (storableManyCSV <| SMT.Formats.UAsset.UAssetStorer()) ]}

                                       
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
                                       [ typedefof<SMT.Formats.TBCR.TBCR>, (storableManyCSV <| SMT.Formats.TBCR.TBCRStorer())
                                         typedefof<SMT.Formats.UAsset.ExtendedUAsset>, (storableManyCSV <| SMT.Formats.UAsset.UAssetStorer()) ]}

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