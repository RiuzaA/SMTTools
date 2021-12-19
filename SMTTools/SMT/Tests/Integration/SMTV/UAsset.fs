module SMT.Tests.Integration.SMTV.UAsset

open Fuchu
open System.Collections.Generic
open System.IO

open SMT.AllGames
open SMT.Formats.UAsset
open SMT.Tests.Environment
open SMT.Utils

[<Tests>]
let tests =
    let roundTripUAsset file =
        let fileName = Path.Combine(Environment.GameRomfs.[smtV.ID], file)
        let memStreamDict = new Dictionary<string, MemoryStream>()
        let config = Utils.writeToMemoryConfig smtV fileName memStreamDict
        let asset  = readData config fileName
        writeData config fileName asset

        let inUassetBytes  = File.ReadAllBytes(fileName)
        let outUassetBytes = memStreamDict.[fileName].ToArray()
        Assert.Equal(".uasset input and output have same bytes", inUassetBytes, outUassetBytes)

        let uexpFileName   = Path.ChangeExtension(fileName, "uexp")
        let inUexpBytes  = File.ReadAllBytes(uexpFileName)
        let outUExpBytes = memStreamDict.[uexpFileName].ToArray()
        Assert.Equal(".uexp input and output have same bytes", inUexpBytes, outUExpBytes) 

    testList "SMTV UAsset integration test"
        [ testCase "roundTrip SkillData" <| fun _ ->
              roundTripUAsset "Project\Content\Blueprints\Gamedata\BinTable\Battle\Skill\SkillData.uasset"
          testCase "roundTrip NKMBaseTable" <| fun _ ->
              roundTripUAsset "Project\Content\Blueprints\Gamedata\BinTable\Devil\NKMBaseTable.uasset"
        ]