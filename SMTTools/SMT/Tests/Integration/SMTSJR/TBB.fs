module SMT.Tests.Integration.SMTSJR.TBB

open Fuchu
open System.Collections.Generic
open System.IO

open SMT.AllGames
open SMT.Formats.MSG
open SMT.Tests.Environment
open SMT.Settings
open SMT.Utils

[<Tests>]
let tests =
    let rountTripEventMessages settings =
        let fileName = Path.Combine(Environment.GameRomfs.[smtSJR.ID], "Enemy/NKMLoreData.mbm")
        let memStreamDict = new Dictionary<string, MemoryStream>()
        let baseConfig = Utils.writeToMemoryConfig smtSJR fileName memStreamDict
        let config = {baseConfig with Settings = settings}
        let data   = readData config fileName
        writeData config fileName data

        let inBytes  = File.ReadAllBytes(fileName)
        let outBytes = memStreamDict.[fileName].ToArray()
        Assert.Equal(".mbm input and output have same bytes", inBytes, outBytes)

    testList "TBB MSG Integration tests"
        [ testCase "roundTrip NKMLoreData.mbm" <| fun _ ->
              rountTripEventMessages defaultSettings
          testCase "roundTrip NKMLoreData.mbm full width conversions" <| fun _ ->
              rountTripEventMessages {defaultSettings with ConvertFromFullWidthInMBM = true}
        ]