module SMT.Tests.Utils

open Fuchu
open System
open System.IO

open SMT.Utils

let writeZeroPaddedStringResults len str =
    let memStream = new MemoryStream()
    let writer    = new BinaryWriter(memStream)
    writer.WriteZeroPaddedString len str
    memStream.ToArray()

[<Tests>]
let tests =
    testList "SMT.Utils"
        [ testCase "WriteZeroPaddedString less than maximum" <| fun _ ->
              let bytes = writeZeroPaddedStringResults 5 "asd"
              Assert.Equal("Will have remaining bytes 0", [|Convert.ToByte('a'); Convert.ToByte('s'); Convert.ToByte('d'); 0uy; 0uy|], bytes)
          testCase "WriteZeroPaddedString equal to maximum" <| fun _ ->
              let bytes = writeZeroPaddedStringResults 3 "asd"
              Assert.Equal("Will have last byte cut off for NULL terminator", [|Convert.ToByte('a'); Convert.ToByte('s'); 0uy|], bytes)
          testCase "WriteZeroPaddedString greater than maximum" <| fun _ ->
              let bytes = writeZeroPaddedStringResults 4 "asdfghjkl"
              Assert.Equal("Will be cut off", [|Convert.ToByte('a'); Convert.ToByte('s'); Convert.ToByte('d'); 0uy|], bytes)
          ]

