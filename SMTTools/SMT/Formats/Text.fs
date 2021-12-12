module SMT.Formats.Text

open System.Globalization
open System.IO
open System.Linq
open System.Text
open System

open SMT.Types
open SMT.Utils

let zeroBytePair = [| 0uy; 0uy |]

let replacePrivateUseChar (ch: char) =
    let format = "X2"
    if CharUnicodeInfo.GetUnicodeCategory ch = UnicodeCategory.PrivateUse then
        $"{{u{(int ch).ToString(format)}}}"
    else
        ch.ToString()

let containsPrivateUse (s: string) = s.ToCharArray() |> Array.exists (fun ch -> CharUnicodeInfo.GetUnicodeCategory ch = UnicodeCategory.PrivateUse)

let encodeAtlusText (config: Config) (strToEnc: string) (writer: BinaryWriter) =
    let startPos = writer.BaseStream.Position
    let sjis = Encoding.GetEncoding("sjis")
    let write c = writer.Write(sjis.GetBytes(c.ToString()))
    let mappingsToChars = Map.insideOut config.Game.OutOfRangeCharMappings
    let mutable idx = 0
    while idx < strToEnc.Length do
        match strToEnc.[idx] with
        | '{' ->
            let endIdx = strToEnc.IndexOf('}', idx)
            if endIdx = -1 then
                log Warning $"Unclosed {{ in string `{strToEnc}`"
                idx <- strToEnc.Length
            else
                let key = strToEnc.Substring(idx + 1, endIdx - idx - 1)
                if key.Length = 0 then failwith "control character is missing between braces {}"
                match key.Chars 0 with
                | 'u' ->
                    Convert.ToByte(key.Substring(1, 2), 16) |> writer.Write
                    Convert.ToByte(key.Substring(3, 2), 16) |> writer.Write
                | _   ->
                    match Map.tryFind key mappingsToChars with
                    | Some c -> writer.Write(int16 c)
                    | None   -> failwith $"Unknown special character mapping {{{key}}}"
                idx <- endIdx + 1
        | c   ->
            write c
            idx <- idx + 1
    int (writer.BaseStream.Position - startPos)

let encodeAtlusTextArray (config: Config) (strToEnc: string) =
    use outputMem = new MemoryStream(strToEnc.Length)
    use writer    = new BinaryWriter(outputMem)
    ignore <| encodeAtlusText config strToEnc writer
    outputMem.ToArray()

let decodeAtlusText (config: Config) (len: int) (reader: BinaryReader) =
    let sjis = Encoding.GetEncoding("sjis")
    let mutable str = ""
    let mutable pair = zeroBytePair
    for _ in 0..(len / 2)-1 do
        pair <- reader.ReadBytes 2
        if pair <> zeroBytePair then
            let sjisPair = sjis.GetString pair
            if not ((sjis.GetBytes sjisPair).SequenceEqual(pair)) || containsPrivateUse sjisPair then
                let ch = 0s ||| int16 pair.[0] ||| (int16 pair.[1] <<< 8) |> char
                match Map.tryFind ch config.Game.OutOfRangeCharMappings with
                | Some rep ->
                    str <- str + $"{{{rep}}}"
                | None ->
                    let format = "X2"
                    str <- str + $"{{u{pair.[0].ToString(format)}{pair.[1].ToString(format)}}}"
            else
                str <- str + sjisPair
    str