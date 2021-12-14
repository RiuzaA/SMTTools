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
    testList "SMTIV TBL Integration tests"
        [ testCase "roundTrip event messages" <| fun _ ->
            let fileName = Path.Combine(Environment.GameRomfs.[smtIV.ID], "battle/NKMSortIndex.tbb")
            let memStreamDict = new Dictionary<string, MemoryStream>()
            let config = Utils.writeToMemoryConfig smtIV fileName memStreamDict
            let data   = readData config fileName
            writeData config fileName data

            let inBytes  = File.ReadAllBytes(fileName)
            let outBytes = memStreamDict.[fileName].ToArray()
            Assert.Equal(".tbb input and output have same bytes", inBytes, outBytes)
        ]