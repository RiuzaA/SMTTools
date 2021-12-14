module SMT.Formats.Text

open System.Globalization
open System.IO
open System.Linq
open System.Text
open System

open SMT.Types
open SMT.Utils

// Character conversion and remapping

let encodeConfigDictChar config c =
    match Map.tryFind c config.Context.EncodeCharPairs with
    | Some c' -> c'
    | None -> c

let encodeConfigDict config = String.map (encodeConfigDictChar config)

let decodeConfigDictChar config c =
    match Map.tryFind c config.Context.DecodeCharPairs with
    | Some c' -> c'
    | None -> c

let decodeConfigDict config = String.map (decodeConfigDictChar config)

let toFullWidthChar config =
    function
    | c when Map.containsKey c config.Context.EncodeCharPairs ->
        Map.find c config.Context.EncodeCharPairs
    | c when c >= '!' && c <= '~' ->
        Convert.ToChar (Convert.ToInt32(c) + 0xFEE0)
    | ' ' -> '　'
    | c -> c

let toFullWidth config = String.map (toFullWidthChar config)

let toHalfWidthChar config =
    function
    | c when Map.containsKey c config.Context.DecodeCharPairs ->
        Map.find c config.Context.DecodeCharPairs
    | c when c >= '！' && c <= '～' ->
        Convert.ToChar (Convert.ToInt32(c) - 0xFEE0)
    | '　' -> ' '
    | c -> c

let toHalfWidth config = String.map (toHalfWidthChar config)

let zeroBytePair = [| 0uy; 0uy |]

let replacePrivateUseChar (ch: char) =
    let format = "X2"
    if CharUnicodeInfo.GetUnicodeCategory ch = UnicodeCategory.PrivateUse then
        $"{{u{(int ch).ToString(format)}}}"
    else
        ch.ToString()

let containsPrivateUse (s: string) = s.ToCharArray() |> Array.exists (fun ch -> CharUnicodeInfo.GetUnicodeCategory ch = UnicodeCategory.PrivateUse)

let shouldConvertFullWidth config = config.Settings.ConvertFromFullWidthInMBM && config.Context.FileExtension.ToLower() = ".mbm"

// Encoding and decoding

let encodeBoundedAtlusText (config: Config) (len: int) (strToEnc: string) (mainWriter: BinaryWriter) =
    let sjis = Encoding.GetEncoding("sjis")
    let encFn =
        if shouldConvertFullWidth config then
            toFullWidthChar config
        else
            encodeConfigDictChar config
    let mappingsToChars = Map.insideOut config.Game.OutOfRangeCharMappings
    use memStream = new MemoryStream(min (int len) (strToEnc.Length * 2))
    use writer    = new BinaryWriter(memStream)
    let mutable lastLenWritten = 0L
    let mutable idx = 0
    let mutable atEnd = false
    while not atEnd && idx < strToEnc.Length do
        let charStart = writer.BaseStream.Position
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
            writer.Write(sjis.GetBytes((encFn c).ToString()))
            idx <- idx + 1
        lastLenWritten <- writer.BaseStream.Position - charStart
        atEnd          <- writer.BaseStream.Position >= int64 len
    if int writer.BaseStream.Position >= len then
        writer.ShiftPosition -lastLenWritten
    writer.Write 0uy    
    let lenToWrite = min (int writer.BaseStream.Position) len
    use reader     = new BinaryReader(memStream)
    reader.JumpTo 0L
    let bytes  = reader.ReadBytes lenToWrite
    mainWriter.Write(bytes)

let encodeAtlusText (config: Config) (strToEnc: string) (writer: BinaryWriter) =
    let startPos = writer.BaseStream.Position
    encodeBoundedAtlusText config Int32.MaxValue strToEnc writer
    int (writer.BaseStream.Position - startPos)

let encodeZeroPaddedAtlusText (config: Config) (len: int) (strToEnc: string) (writer: BinaryWriter) =
    let startPos = writer.BaseStream.Position
    encodeBoundedAtlusText config len strToEnc writer
    writer.PadZeros <| len - int (writer.BaseStream.Position - startPos)

let decodeAtlusText (config: Config) (len: int) (reader: BinaryReader) =
    let sjis = Encoding.GetEncoding("sjis")
    let mutable str = ""
    let mutable pair = zeroBytePair
    let mutable iter = 0
    let mutable rem: byte option = None
    while iter < len do
        pair <-
            match rem with
            | Some r ->
                let p = [|r; if iter + 1 >= len then 0uy else reader.ReadByte ()|]
                rem <- None
                p
            | None when iter + 1 >= len ->
                    [|reader.ReadByte (); 0uy|]
            | None ->
                    reader.ReadBytes 2
        if pair.[0] <> 0uy || config.Game.AllowNullCharsInMBM then
             if pair.[0] < 127uy then
                 str <- str + sjis.GetString [|pair.[0]|]
                 iter <- iter - 1
                 rem  <- Some pair.[1]
             else
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
        iter <- iter + 2
    while str.[str.Length - 1] = '\u0000' do
        str <- str.Substring(0, str.Length - 1)
    str |> if shouldConvertFullWidth config then toHalfWidth config else decodeConfigDict config