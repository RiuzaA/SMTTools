module SMT.Types

open System
open System.IO
open System.Collections.Immutable
open System.Text
open Newtonsoft.Json

open SMT.Settings
open SMT.TypeMap
open Newtonsoft.Json.Linq

type CSVUnpackAttribute(prefix: string) =
    inherit Attribute()

    new() = CSVUnpackAttribute("")

    member self.Prefix with get () = prefix

type CSVCellConverterAttribute(func: string) =
    inherit Attribute()

    member self.Function with get () = func

type LogType = Info | Debug | Warning | Error

let log t s = printfn $"log {t.ToString()}: {s}"

type IBytesLike =
    abstract member ToBytes : unit -> byte array

type TableID = {File: string; TableNum: int}

type IStorable<'a> =
    abstract member Read<'a>  : Config -> BinaryReader -> 'a
    abstract member Write<'a> : Config -> 'a -> BinaryWriter -> unit

and IHeader =
    abstract member IsHeaderMatching : Config -> BinaryReader -> bool

and Game =
    { ID:                     String
      Name:                   string
      Storers:                ImmutableTypedMap<Type>
      Sections:               Map<TableID, string>
      TableRowConverters:     Map<TableID, IStorable<obj>>
      CSVConverters:          CSVConverters
      ManyCSVConverters:      ManyCSVConverters
      OutOfRangeCharMappings: Map<char, string>
      PrivateUseCharMappings: Map<char, string>
      AllowNullCharsInMBM:    bool}
and ConfigContext =
    { BaseFileName:       string // file name up until first extension, resembling just the game file (i.e. CoolData.tbb.patch.json's base is CoolData.tbb)
      FullFileName:       string // entire file name;
      FileExtension:      string
      FilePath:           string
      SubFileIdx:         int
      SameFileValues:     ImmutableTypedMap<int>
      AllStorableFormats: StorableFormat list
      GetFileWriter:      string -> BinaryWriter 
      DecodeCharPairs:    Map<char, char>
      EncodeCharPairs:    Map<char, char> }
and Config =
    { Game:      Game
      Context:   ConfigContext
      Settings:  Settings }

and StorableFormat(header: IHeader, storable: IStorable<obj>) =
    interface IStorable<obj> with
        member self.Read config reader = storable.Read config reader
        member self.Write config a writer = storable.Write config a writer
    interface IHeader with
        member self.IsHeaderMatching config reader = header.IsHeaderMatching config reader

and ICSV<'a> =
    abstract member CSVHeader : Config -> 'a -> string array
    abstract member CSVRows   : Config -> 'a -> string array array

and IManyCSV<'a> =
    abstract member WriteCSVFiles : Config -> 'a -> string -> unit

and StorableCSV(csv: ICSV<obj>, storable: IStorable<obj>) =
    interface IStorable<obj> with
        member self.Read config reader = storable.Read config reader
        member self.Write config a writer = storable.Write config a writer
    interface ICSV<obj> with
        member self.CSVHeader config data = csv.CSVHeader config data
        member self.CSVRows config data   = csv.CSVRows config data
        
and StorableManyCSV(csv: IManyCSV<obj>, storable: IStorable<obj>) =
    interface IStorable<obj> with
        member self.Read config reader = storable.Read config reader
        member self.Write config a writer = storable.Write config a writer
    interface IManyCSV<obj> with
        member self.WriteCSVFiles config data path = csv.WriteCSVFiles config data path

and BytesStorer(n: int) =
    interface IStorable<byte array> with
        member self.Read _ reader    = reader.ReadBytes n
        member self.Write _ a writer = writer.Write a
    interface ICSV<byte array> with
        member self.CSVHeader _ data = Array.mapi (fun idx b -> idx.ToString("X2")) data
        member self.CSVRows _ data   = [| Array.mapi (fun idx b -> (int b).ToString("X2")) data |]
    interface IManyCSV<byte array> with
        member self.WriteCSVFiles config data path =
            let csv = self :> ICSV<byte array>
            let headerStr = System.String.Join(',', csv.CSVHeader config data)
            let rowsStr   = System.String.Join('\n', Array.map (fun row -> System.String.Join(',', row :> string array)) <| csv.CSVRows config data)
            let csvBytes = Encoding.UTF8.GetBytes($"{headerStr}\n{rowsStr}")
            File.Create($"{path}/{config.Context.FullFileName}.0.csv").Write(csvBytes, 0, csvBytes.Length)

and CSVConverters'<'a>(dict: ImmutableDictionary<Type, 'a>, defConverter: 'a) =
    member self.Add (t: Type) converter = CSVConverters'<'a>(dict.Add(t, converter), defConverter)

    member self.Get (t: Type) = dict.GetValueOrDefault(t, defConverter)

    member self.GetOrThrow (t: Type) = dict.[t]

    member self.TryGet (t: Type) =
        if dict.ContainsKey t then
            Some dict.[t]
        else
            None

    member self.GetFor<'b> (b: 'b) = dict.GetValueOrDefault(b.GetType(), defConverter)

    member self.GetForOrThrow<'b> (b: 'b) = dict.[b.GetType()]
and CSVConverters = CSVConverters'<StorableCSV>
and ManyCSVConverters = CSVConverters'<StorableManyCSV>

type OrUnknown<'a, 'b> =
    | Known of 'a
    | Unknown of 'b

type HexBytesConverter() =
    inherit JsonConverter()

    override self.CanConvert t = t = typedefof<byte array>

    override self.CanRead with get () = true
    override self.CanWrite with get () = true
    
    override self.ReadJson(reader, oType, existingVal, serializer) =
        if reader.TokenType = JsonToken.StartArray then
            let readVal (v: JToken) =
                match v.Type with
                | JTokenType.String  -> Convert.ToString(v)
                | JTokenType.Integer -> Convert.ToInt32(v).ToString("X2")
                | e -> failwith $"Unable to parse token of type {e}"
            let token = JToken.Load reader
            let array = Array.ofSeq token
                     |> Array.map (readVal >> (fun s -> Convert.ToByte(s, 16)))
            array :> obj
        else
            failwith $"Unable to parse token of type {reader.TokenType}"

    override self.WriteJson(writer, value, serializer) =
        match value with
        | :? array<byte> as v ->
            JArray(Array.map (fun b -> (int b).ToString("X2")) v).WriteTo(writer)
        | _ -> failwith $"Unable to write json for type {value.GetType()}"

type Export<'a> =
    { GameID:       string
      BaseFileName: string
      Type:         Type
      Data:         'a }