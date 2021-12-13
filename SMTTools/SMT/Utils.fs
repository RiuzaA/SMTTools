module SMT.Utils

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open Microsoft.FSharp.NativeInterop

open SMT.TypeMap
open SMT.Types
open System.Collections.Immutable
open System.Reflection

// obj conversion

let objStorable<'a, 'b when 'b :> IStorable<'a>> (b: 'b) =
    { new IStorable<obj> with
          member self.Read config reader = b.Read config reader :> obj
          member self.Write config value reader = b.Write config (value :?> 'a) reader }

let objCSV<'a, 'b when 'b :> ICSV<'a>> (b: 'b) =
    { new ICSV<obj> with
          member self.CSVHeader config data = b.CSVHeader config (data :?> 'a)
          member self.CSVRows config data = b.CSVRows config (data :?> 'a) }

let objManyCSV<'a, 'b when 'b :> IManyCSV<'a>> (b: 'b) =
    { new IManyCSV<obj> with
          member self.WriteCSVFiles config data file = b.WriteCSVFiles config (data :?> 'a) file }

//

let storableFormat<'a, 'b when 'b :> IHeader and 'b :> IStorable<'a>> (b: 'b) =
    StorableFormat(b, objStorable b)

let findFirstReadableFormat (config: Config) (reader: BinaryReader) =
    List.tryFind (fun f -> (f :> IHeader).IsHeaderMatching config reader) config.Context.AllStorableFormats

let findFirstWritableFormat<'a> (config: Config) (data: 'a) =
    config.Game.Storers.TryGet<obj IStorable> (data.GetType())

let isHeaderMatching<'a when 'a :> IHeader> (a: 'a) = a.IsHeaderMatching

let isHeaderEq (expected: string) (reader: BinaryReader) =
    let headerStr = reader.ReadBytes(expected.Length) |> Encoding.ASCII.GetString
    reader.BaseStream.Position <- reader.BaseStream.Position - int64 expected.Length
    headerStr = expected

let isHeaderBytes (expected: byte array) (reader: BinaryReader) =
    let headerBytes = reader.ReadBytes(expected.Length)
    reader.BaseStream.Position <- reader.BaseStream.Position - int64 expected.Length
    headerBytes = expected

let storableCSV<'a, 'b when 'b :> IStorable<'a> and 'b :> ICSV<'a>> (b: 'b) = StorableCSV(objCSV b, objStorable b)
let storableManyCSV<'a, 'b when 'b :> IStorable<'a> and 'b :> IManyCSV<'a>> (b: 'b) = StorableManyCSV(objManyCSV b, objStorable b)

let iRead<'a, 'b when 'b :> IStorable<'a>> (cfg: Config) (storer: 'b) = (storer :> IStorable<'a>).Read cfg
let iWrite<'a, 'b when 'b :> IStorable<'a>> (cfg: Config) (storer: 'b) (a: 'a) = (storer :> IStorable<'a>).Write cfg a

let structFromBytes<'a> (bytes: byte array) = 
    use buffer = fixed bytes
    let intptr = NativePtr.toNativeInt buffer
    Marshal.PtrToStructure<'a>(intptr);

let bytesFromStruct<'a> (a: 'a) =
    let len    = Marshal.SizeOf(a)
    let bytes  = Array.create len 0uy
    let intptr = Marshal.AllocHGlobal len
    Marshal.StructureToPtr(a, intptr, true)
    Marshal.Copy(intptr, bytes, 0, len)
    Marshal.FreeHGlobal(intptr)
    bytes
    
let structRead<'a> (reader: BinaryReader): 'a = reader.ReadBytes(Marshal.SizeOf<'a>()) |> structFromBytes

let structWrite<'a> (writer: BinaryWriter) (a: 'a) = writer.Write(bytesFromStruct a)

let rec refCSVHeader<'a> =
    let rec names prefixes (p: PropertyInfo) =
            if p.PropertyType.GetCustomAttribute(typedefof<CSVUnpackAttribute>, true) <> null then
                p.PropertyType.GetProperties() |> Array.map (names (p.Name :: prefixes)) |> Array.concat
            else
                [| System.String.Join(".", List.rev (p.Name :: prefixes)) |]
    (typedefof<'a>).GetProperties() |> Array.map (names []) |> Array.concat

// Types

type AnyStorer() =
    interface IStorable<obj> with
        member self.Read _ _ = failwith "Unable to read obj from binary; not enough type information"
        member self.Write config data writer =
            let bytes = match data with
                        | :? (byte array) as bytes -> bytes
                        | s when (s.GetType()).IsValueType -> bytesFromStruct s
                        | e -> failwith $"unable to get bytes for type `{e.GetType()}`"
            ((BytesStorer(bytes.Length)) :> IStorable<byte array>).Write config bytes writer
    interface ICSV<obj> with
        member self.CSVHeader config data =
            let bytes = match data with
                        | :? (byte array) as bytes -> bytes
                        | :? IBytesLike as bytesLike -> bytesLike.ToBytes()
                        | s when (s.GetType()).IsValueType -> bytesFromStruct s
                        | e -> failwith $"unable to get bytes for type `{e.GetType()}`"
            ((BytesStorer(bytes.Length)) :> ICSV<byte array>).CSVHeader config bytes
        member self.CSVRows config data =
            let bytes = match data with
                        | :? (byte array) as bytes -> bytes
                        | :? IBytesLike as bytesLike -> bytesLike.ToBytes()
                        | s when (s.GetType()).IsValueType -> bytesFromStruct s
                        | e -> failwith $"unable to get bytes for type `{e.GetType()}`"
            ((BytesStorer(bytes.Length)) :> ICSV<byte array>).CSVRows config bytes
    interface IManyCSV<obj> with
        member self.WriteCSVFiles config data path =
            let bytes = match data with
                        | :? (byte array) as bytes -> bytes
                        | :? IBytesLike as bytesLike -> bytesLike.ToBytes()
                        | s when (s.GetType()).IsValueType -> bytesFromStruct s
                        | e -> failwith $"unable to get bytes for type `{e.GetType()}`"
            ((BytesStorer(bytes.Length)) :> IManyCSV<byte array>).WriteCSVFiles config bytes path

let enumOrUnknown<'a, 'i when 'a : enum<'i>> (i: 'i) : OrUnknown<'a, 'i> =
    let enum = LanguagePrimitives.EnumOfValue<'i, 'a>(i)
    if System.Enum.IsDefined(typedefof<'a>, enum)
    then Known enum
    else Unknown i

module OrUnknown =
    let ofEnum<'a, 'i when 'a : enum<'i>> (i: 'i) : OrUnknown<'a, 'i> = enumOrUnknown i

    let simpleToString = function
        | Known a   -> a.ToString()
        | Unknown i -> "Unknown " + i.ToString()

    let unwrapOrErr = function
        | Known a   -> a
        | Unknown a -> failwith <| "OrUnknown.unwrap Unknown value `" + a.ToString() + "`"

let mkExport config data = {GameID = config.Game.ID; BaseFileName = config.Context.BaseFileName; Type = data.GetType(); Data = data}

let readData config file =
    use reader = new BinaryReader(File.OpenRead(file))
    match findFirstReadableFormat config reader with
    | None ->
        failwith "No format found for parsing file"
    | Some storer ->
        iRead config storer reader

let writeData config file (data: obj) =
    match config.Game.Storers.TryGet<obj IStorable> (data.GetType()) with
    | None ->
        failwith $"No format found for storing file of type {data.GetType()}"
    | Some storer ->
        use writer = config.Context.GetFileWriter file
        iWrite config storer data writer

// Defaults

let defaultGame =
    { ID                     = "Unknown"
      Name                   = "Unknown"
      Storers                = ImmutableTypedMap()
      Sections               = Map.empty
      TableRowConverters     = Map.empty
      CSVConverters          = CSVConverters(ImmutableDictionary.Create(), StorableCSV(objCSV (AnyStorer()), objStorable (AnyStorer())))
      ManyCSVConverters      = ManyCSVConverters(ImmutableDictionary.Create(), StorableManyCSV(objManyCSV (AnyStorer()), objStorable (AnyStorer())))
      OutOfRangeCharMappings = Map.empty
      PrivateUseCharMappings = Map.empty }
let configFromFile (filepath: string) (storableFormats: StorableFormat list) =
    let name         = Path.GetFileName filepath
    let firstDotIdx  = name.IndexOf '.'
    let secondDotIdx = if firstDotIdx = -1 then -1 else name.IndexOf('.', firstDotIdx + 1)
    let baseFile     = if secondDotIdx = -1 then name else name.Substring(0, secondDotIdx)
    { Game = defaultGame
      Context = { BaseFileName       = baseFile
                  FullFileName       = name
                  FilePath           = filepath
                  FileExtension      = Path.GetExtension(name)
                  SubFileIdx         = 0
                  SameFileValues     = ImmutableTypedMap()
                  AllStorableFormats = storableFormats
                  GetFileWriter      = fun file ->
                      (new FileInfo(file)).Directory.Create()
                      new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write))}}

// Extensions

type BinaryReader with
    member reader.ReadStruct<'a> () = structRead<'a> reader

    member reader.ReadZeroPaddedString (len: int) = Encoding.ASCII.GetString(reader.ReadBytes(len)).TrimEnd '\u0000'

    member reader.JumpTo (i: int64) = reader.BaseStream.Position <- i

    member reader.PeekBytes (count: int) =
        let bytes = reader.ReadBytes count
        reader.ShiftPosition (int64 -count)
        bytes

    member reader.ShiftPosition (i: int64) = reader.BaseStream.Position <- reader.BaseStream.Position + i

type BinaryWriter with
    member writer.EnsureSize<'a> (size: int) (f: unit -> 'a) =
        let startPos = writer.BaseStream.Position
        let r = f ()
        let endPos = writer.BaseStream.Position
        if (endPos - startPos <> int64 size) then
            failwith $"Expected size {size}, but wrote size of {endPos - startPos}"
        r

    member writer.PadZeros n =
        for _ = 0 to n-1 do
            writer.Write 0uy

    member writer.WriteStruct<'a>(a: 'a) = structWrite<'a> writer a

    member writer.JumpTo (i: int64) = writer.BaseStream.Position <- i

    member writer.WriteEnum<'a when 'a : enum<byte>> (a: 'a) = writer.Write (LanguagePrimitives.EnumToValue a)
    member writer.WriteEnumUInt16<'a when 'a : enum<uint16>> (a: 'a) = writer.Write (LanguagePrimitives.EnumToValue a)
    member writer.WriteEnumInt32<'a when 'a : enum<int32>> (a: 'a) = writer.Write (LanguagePrimitives.EnumToValue a)
    member writer.WriteEnumUInt32<'a when 'a : enum<uint32>> (a: 'a) = writer.Write (LanguagePrimitives.EnumToValue a)

    member writer.WriteUnknown<'a when 'a : enum<byte>> (v: OrUnknown<'a, byte>) =
        match v with
        | Known a   -> writer.Write (byte (LanguagePrimitives.EnumToValue a))
        | Unknown a -> writer.Write a

    member writer.WriteZeroPaddedString (len: int) (str: string) =
        let targetPos = writer.BaseStream.Position + int64 len
        let cutStr = if str.Length > len then str.Substring(0, len) else str
        for c in cutStr.ToCharArray() do
            writer.Write(Convert.ToByte c)
        while (writer.BaseStream.Position < targetPos) do
            writer.Write 0uy

module Array =
    let csvToString array = "[" + String.Join("; ", Array.map (fun v -> v.ToString()) array) + "]"

    let replicateif<'a> (n: int) (f: int -> 'a) : 'a array = Array.map f [|0..n-1|]
    let replicatef<'a> (n: int) (f: unit -> 'a) : 'a array = replicateif n (fun _ -> f ())
    let foldi<'a, 'b> (f: 'b -> int -> 'a -> 'b) (acc: 'b) (arr: 'a array) = Array.fold (fun acc idx -> f acc idx arr.[idx]) acc [| 0..(arr.Length-1) |]

module List =
    let replicateif<'a> (n: int) (f: unit -> 'a) : 'a list = List.map (fun _ -> f ()) [0..n-1]
    let replicatef<'a> (n: int) (f: unit -> 'a) : 'a list = replicateif n (fun _ -> f ())

module Map =
    let csvToString map = "[" + String.Join("; ", Map.toArray map |> Array.map (fun (k, v) -> k.ToString() + "->" + v.ToString())) + "]"

    let insideOut<'k, 'v when 'k : comparison and 'v: comparison>  (map: Map<'k, 'v>) : Map<'v, 'k> = Map.fold (fun acc k v -> Map.add v k acc) Map.empty map

module String =
    let sanitizeControlChars (s: String) =
        let replaceControl c = let hex = (int c).ToString("X2") in $"{{{hex}}}"
        Array.fold (fun acc c -> if Char.IsControl c then acc + replaceControl c else acc + c.ToString()) "" <| s.ToCharArray ()

module Enum =
    let toArray<'a, 'i when 'a : enum<'i>> =
        Enum.GetValues(typedefof<'a>) :?> ('a array)
    let toList<'a, 'i when 'a : enum<'i>> =
        Enum.GetValues(typedefof<'a>) :?> ('a array) |> List.ofArray
    let toSet<'a, 'i when 'a : enum<'i> and 'a : comparison> =
        Enum.GetValues(typedefof<'a>) :?> ('a array) |> Set.ofArray

module Flag =
    let inline toSet (a: ^a) : ^a Set =
        let fields = Enum.toSet< ^a, ^i>
        Set.filter (fun f -> f &&& a <> LanguagePrimitives.EnumOfValue LanguagePrimitives.GenericZero) fields

    let inline ofSet (set: ^a Set) : ^a = Set.fold (fun acc f -> acc ||| f) (LanguagePrimitives.EnumOfValue (LanguagePrimitives.GenericOne : ^i)) set

module Marshal =
    let size<'a> = Marshal.SizeOf(typedefof<'a>)