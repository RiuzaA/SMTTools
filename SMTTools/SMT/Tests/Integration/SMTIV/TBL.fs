module SMT.Tests.Integration.SMTIV.TBL

open Fuchu
open System.Collections.Generic
open System.IO

open SMT.AllGames
open SMT.Formats.TBL
open SMT.Tests.Environment
open SMT.Utils

[<Tests>]
let tests =
    let roundTrip file =
        let fileName = Path.Combine(Environment.GameRomfs.[smtIV.ID], file)
        let memStreamDict = new Dictionary<string, MemoryStream>()
        let config = Utils.writeToMemoryConfig smtIV fileName memStreamDict
        let data   = readData config fileName
        writeData config fileName data

        let inBytes  = File.ReadAllBytes(fileName)
        let outBytes = memStreamDict.[fileName].ToArray()
        Assert.Equal(".tbb input and output have same bytes", inBytes, outBytes)
    testList "SMTIV TBL Integration tests"
        [ testCase "roundTrip event messages" <| fun _ ->
            roundTrip "battle/NKMSortIndex.tbb"
          testCase "roundTrip NpcRoom" <| fun _ ->
              roundTrip "npc_room/NpcRoomTable.tbb"
          testCase "roundTrip quest NpcHaichiTable" <| fun _ -> roundTrip "quest_center/common/NpcHaichiTable.tbb"
          testCase "roundTrip quest QcName"         <| fun _ -> roundTrip "quest_center/common/QcName.tbb"
          testCase "roundTrip quest RankingTable"   <| fun _ -> roundTrip "quest_center/common/RankingTable.tbb"
        ]