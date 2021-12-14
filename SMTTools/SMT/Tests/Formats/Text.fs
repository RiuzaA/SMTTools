module SMT.Tests.Formats.Text

open Fuchu
open System
open System.IO
open SMT.Formats.Text
open SMT.AllGames
open SMT.Settings
open SMT.Utils

let encodesInto config (str: string) (bytes: byte array) =
    use outputMem = new MemoryStream()
    use writer    = new BinaryWriter(outputMem)
    let encoded   = encodeAtlusText config str writer
    Assert.Equal($"encoded matches bytes for `{str}`", bytes, outputMem.ToArray())

let decodesFrom config (bytes: byte array) (str: string) =
    use inputMem   = new MemoryStream(bytes)
    use reader     = new BinaryReader(inputMem)
    let decodedStr = decodeAtlusText config bytes.Length reader
    Assert.Equal($"decoded matches string for `{str}`", str, decodedStr)

let roundTripEncodeDecode config (str: string) (bytes: byte array) =
    encodesInto config str bytes
    decodesFrom config bytes str

[<Tests>]
let tests =
    let spanishExamplePairs = Map.ofList ['ン', 'ñ'; 'プ', '¿']
    let smtSJRConfig = {configFromFile "DummyTestFile.mbm" defaultSettings [] with Game = smtIV}
    let smtIVConfig = {configFromFile "DummyTestFile.mbm" defaultSettings [] with Game = smtIV}
    let smtIVSpanishConfig = {smtIVConfig with Context = {smtIVConfig.Context with DecodeCharPairs = spanishExamplePairs
                                                                                   EncodeCharPairs = Map.insideOut spanishExamplePairs}}
    testList "SMT.Formats.Text"
        [ testCase "roundTripEncodeDecodePlain"      <| fun _ ->
              roundTripEncodeDecode smtSJRConfig "Ｄｕｍｍｙ" [|0x82uy; 0x63uy; 0x82uy; 0x95uy; 0x82uy; 0x8Duy; 0x82uy; 0x8Duy; 0x82uy; 0x99uy; 0x00uy|]
          testCase "roundTripEncodeDecodeWithSpaces" <| fun _ ->
              roundTripEncodeDecode smtSJRConfig "Ｆｉｒｅ　Ｂｒｅａｔｈ" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x81uy; 0x40uy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy; 0x00uy|]
          testCase "roundTripEncodeDecodeUnicodeEscape" <| fun _ ->
              roundTripEncodeDecode smtSJRConfig "Ｆｉｒｅ{uE60C}Ｂｒｅａｔｈ" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xE6uy; 0x0Cuy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy; 0x00uy|]
          testCase "roundTripEncodeDecodeOutOfRangeCharMappings" <| fun _ ->
              roundTripEncodeDecode smtSJRConfig "Ｆｉｒｅ{NL}Ｂｒｅａｔｈ" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xF8uy; 0x01uy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy; 0x00uy|]
          testCase "roundTripEncodeDecodeOutOfRangeCharMappings too long" <| fun _ ->
              let str   = "Ｆｉｒｅ{NL}Ｂｒｅａｔｈ"
              let bytes = [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xF8uy; 0x01uy; 0x00uy|]
              use memStream = new MemoryStream()
              use writer    = new BinaryWriter(memStream)
              encodeZeroPaddedAtlusText smtIVConfig 11 str writer
              Assert.Equal($"encoded matches cut off bytes for `{str}`", bytes, memStream.ToArray())
          testCase "encodeZeroPaddedAtlusText pad zeros" <| fun _ ->
              let str   = "Ｆｉｒｅ{NL}"
              let bytes = [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xF8uy; 0x01uy; 0x00uy; 0x00uy; 0x00uy; 0x00uy; 0x00uy|]
              use memStream = new MemoryStream()
              use writer    = new BinaryWriter(memStream)
              encodeZeroPaddedAtlusText smtIVConfig 15 str writer
              Assert.Equal($"encoded matches cut off bytes for `{str}`", bytes, memStream.ToArray())
          testCase "roundTripEncodeDecodeOutOfRangeCharMappings with full width conversion" <| fun _ ->
              let config = {smtSJRConfig with Settings = {smtSJRConfig.Settings with ConvertFromFullWidthInMBM = true}}
              roundTripEncodeDecode config "Fire{NL}Breath" [|0x82uy; 0x65uy; 0x82uy; 0x89uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0xF8uy; 0x01uy; 0x82uy; 0x61uy; 0x82uy; 0x92uy; 0x82uy; 0x85uy; 0x82uy; 0x81uy; 0x82uy; 0x94uy; 0x82uy; 0x88uy; 0x00uy|]
          testCase "decodeWithAsciiMixed" <| fun _ ->
              let bytes = [| 0x46uy; 0x83uy; 0x44uy; 0x72uy; 0x69uy; 0x61uy; |]
              decodesFrom smtIVConfig bytes "Fゥria"
          testCase "decodeWithEndingNulls" <| fun _ ->
              let bytes = [|0x82uy; 0x63uy; 0x82uy; 0x95uy; 0x82uy; 0x8Duy; 0x82uy; 0x8Duy; 0x82uy; 0x99uy; 0x00uy; 00uy; 00uy; 00uy; 00uy|]
              decodesFrom smtIVConfig bytes "Ｄｕｍｍｙ"
          testCase "half to full width" <| fun _ ->
              let half     = "ABC! xyz@ 123~"
              let full     = toFullWidth smtIVConfig half
              let expected = "ＡＢＣ！　ｘｙｚ＠　１２３～"
              Assert.Equal("Converts into full width", expected, full)
          testCase "full to half width" <| fun _ ->
              let full     = "ＡＢＣ！　ｘｙｚ＠　１２３～"
              let half     = toHalfWidth smtIVConfig full
              let expected = "ABC! xyz@ 123~"
              Assert.Equal("Converts into half width", expected, half)
          testCase "half to full width with dictionary" <| fun _ ->
              let half     = "¿Español?"
              let full     = toFullWidth smtIVSpanishConfig half
              let expected = "プＥｓｐａンｏｌ？"
              Assert.Equal("Converts non-ascii chars into overriden katakana", expected, full)
          testCase "full to half width with dictionary" <| fun _ ->
              let full     = "プＥｓｐａンｏｌ？"
              let half     = toHalfWidth smtIVSpanishConfig full
              let expected = "¿Español?"
              Assert.Equal("Converts katakana into non-ascii chars", expected, half)
          ]