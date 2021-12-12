module SMT.Tests.Formats.Text

open Fuchu
open System
open System.IO
open SMT.Formats.Text
open SMT.AllGames
open SMT.Utils

let encodesInto game (str: string) (bytes: byte array) =
    let config    = {configFromFile "DummyTestFile" [] with Game = game}
    let encoded   = encodeAtlusTextArray config str
    Assert.Equal($"encoded matches bytes for `{str}`", bytes, encoded)

let decodesFrom game (bytes: byte array) (str: string) =
    let config     = {configFromFile "DummyTestFile" [] with Game = game}
    use inputMem   = new MemoryStream(bytes)
    use reader     = new BinaryReader(inputMem)
    let decodedStr = decodeAtlusText config bytes.Length reader
    Assert.Equal($"decoded matches string for `{str}`", str, decodedStr)

let roundTripEncodeDecode game (str: string) (bytes: byte array) =
    encodesInto game str bytes
    decodesFrom game bytes str

[<Tests>]
let tests =
    testList "SMT.Formats.Text"
        [ testCase "roundTripEncodeDecodePlain"      <| fun _ ->
              roundTripEncodeDecode smtSJR "Ｄｕｍｍｙ" [|0x82uy; 0x63uy; 0x82uy; 0x95uy; 0x82uy; 0x8Duy; 0x82uy; 0x8Duy; 0x82uy; 0x99uy;|]
          testCase "roundTripEncodeDecodeWithSpaces" <| fun _ ->
              roundTripEncodeDecode smtSJR "Ｆｉｒｅ　Ｂｒｅａｔｈ" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x81uy; 0x40uy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy|]
          testCase "roundTripEncodeDecodeUnicodeEscape" <| fun _ ->
              roundTripEncodeDecode smtSJR "Ｆｉｒｅ{uE60C}Ｂｒｅａｔｈ" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xE6uy; 0x0Cuy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy|]
          testCase "roundTripEncodeDecodeOutOfRangeCharMappings" <| fun _ ->
              roundTripEncodeDecode smtSJR "Ｆｉｒｅ{NL}Ｂｒｅａｔｈ" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xF8uy; 0x01uy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy|]
          ]