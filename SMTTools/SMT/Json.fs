module SMT.Json

open JsonDiffPatchDotNet
open JsonDiffPatchDotNet.Formatters.JsonPatch
open Microsoft.AspNetCore.JsonPatch
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.IO
open UAssetAPI

open SMT.Utils
open SMT.Types

// huge warning for people who see this in the future: Newtonsoft fails to deserialize structs and leaves all values 0
let jsonSettings =
    let settings = new JsonSerializerSettings()
    settings.Formatting <- Formatting.Indented
    settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter())
    settings.Converters.Add(HexBytesConverter())
    // UAssetAPI required
    settings.Converters.Add(FSignedZeroJsonConverter())
    settings.Converters.Add(FNameJsonConverter())
    settings.Converters.Add(FStringJsonConverter())
    settings.Converters.Add(FPackageIndexJsonConverter())
    settings.Converters.Add(Newtonsoft.Json.Converters.StringEnumConverter())
    settings.TypeNameHandling      <- TypeNameHandling.Objects
    settings.NullValueHandling     <- NullValueHandling.Include
    settings.FloatParseHandling    <- FloatParseHandling.Double
    settings.ReferenceLoopHandling <- ReferenceLoopHandling.Ignore
    settings

let writeJsonFile file data =
    (new FileInfo(file)).Directory.Create()
    File.WriteAllText(file, JsonConvert.SerializeObject(data, jsonSettings))

let writeJsonPatchFile file dataOrig dataNew =
    let jsonOrig = JsonConvert.SerializeObject(dataOrig, jsonSettings)
    let jsonNew  = JsonConvert.SerializeObject(dataNew, jsonSettings)
    let diffObj = (new JsonDiffPatch()).Diff(jsonOrig, jsonNew)
    let diff = JsonConvert.DeserializeObject<Linq.JToken>(if diffObj = null then "[]" else diffObj)
    writeJsonFile file ((new JsonDeltaFormatter()).Format(diff))

let applyJsonPatch (config: Config) (patchDoc: JsonPatchDocument) (origData: obj) =
    // Yes, these conversion are neccessary
    // JsonPatch doesn't support Sum Types like TBLEntries, and throws an exception when it tries to patch
    // But this hack makes it work
    let exportData = mkExport config origData
    let jobject = JObject.FromObject(exportData, JsonSerializer.Create(jsonSettings))
    patchDoc.ApplyTo(jobject)
    jobject.Last.First.ToObject(exportData.Type, JsonSerializer.Create(jsonSettings))