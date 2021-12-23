open Microsoft.AspNetCore.JsonPatch
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open System.IO
open System.Text
open System
open Fuchu

open SMT.AllGames
open SMT.Json
open SMT.Settings
open SMT.Tests.Environment
open SMT.Types
open SMT.Utils

let loadSettings () =
    let file = "settings.json"
    if File.Exists file then
        JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file))
    else
        writeJsonFile file defaultSettings
        defaultSettings

let getGame (gameID: string) =
    match Map.tryFind (gameID.ToUpper()) SMT.AllGames.GamesByID with
    | Some game -> game
    | None      -> failwith $"Unknown game with ID = `{gameID}`"

let loadAndApplyPatch game settings origfile patchfile outfile = 
    let config      = {configFromFile origfile settings genericStorableFormats with Game = game}
    let origData    = readData config origfile
    let patchDoc    = JsonConvert.DeserializeObject<JsonPatchDocument>(File.ReadAllText(patchfile), jsonSettings)
    let patchedData = applyJsonPatch config patchDoc origData
    let file =
        if Directory.Exists(outfile) then
            $"{outfile}/{config.Context.FullFileName}{config.Context.FileExtension}"
        else
            outfile
    let outConfig   = {configFromFile file settings genericStorableFormats with Game = game}
    writeData outConfig file patchedData

[<STAThread>][<EntryPoint>]
let main argv =
    Encoding.RegisterProvider CodePagesEncodingProvider.Instance
    let settings = loadSettings ()
    match argv with
    | [| gameID; "build_json"; infile; outfile |] ->
        let game   = getGame gameID
        let inConfig = {configFromFile infile settings genericStorableFormats with Game = game}
        let exportData = JsonConvert.DeserializeObject<Export<JObject>>(File.ReadAllText(infile))
        let data = exportData.Data.ToObject(exportData.Type, JsonSerializer.Create(jsonSettings))
        let file =
            if Directory.Exists(outfile) then
                $"{outfile}/{inConfig.Context.FullFileName}.json"
            else
                outfile
        let outConfig = {configFromFile outfile settings genericStorableFormats with Game = game}
        writeData outConfig outfile data
        0
    | [| gameID; "extract_json"; infile; outfile |] ->
        let game   = getGame gameID
        let config = {configFromFile infile settings genericStorableFormats with Game = game}
        let data   = readData config infile
        let file =
            if Directory.Exists(outfile) then
                $"{outfile}/{config.Context.FullFileName}.json"
            else
                outfile
        writeJsonFile file <| mkExport config data
        0
    | [| gameID; "extract_csv"; infile; outdir |] ->
        let game   = getGame gameID
        let config = {configFromFile infile settings genericStorableFormats with Game = game}
        let data   = readData config infile
        let manyWriter = config.Game.ManyCSVConverters.GetOrThrow (data.GetType())
        (manyWriter :> IManyCSV<obj>).WriteCSVFiles config data outdir
        0
    | [| gameID; "apply_patch"; origfile; patchfile; outfile |] ->
        let game        = getGame gameID
        loadAndApplyPatch game settings origfile patchfile outfile
        0
    | [| gameID; "apply_patches"; gameDir; patchDir; buildDir |] ->
        let game       = getGame gameID
        let bsPatchDir = patchDir.Replace("/", "\\") + "\\"
        let patchFiles = Directory.GetFiles(patchDir, "*.patch.json", SearchOption.AllDirectories)
        for patchFile in patchFiles do
            let baseFile = patchFile.Replace(bsPatchDir, "").Replace(".patch.json", "")
            let gameBaseFile = Path.Combine(gameDir, baseFile)
            if File.Exists gameBaseFile then
                loadAndApplyPatch game settings gameBaseFile patchFile (Path.Combine(buildDir, baseFile))
                printfn $"Patched file {baseFile}"
            else
                printfn $"Failed to find base file {gameBaseFile} for json patch {patchFile}"
            ()
        0
    | [| gameID; "diff_files"; origfile; newfile; outfile |] ->
        let game       = getGame gameID
        let origConfig = {configFromFile origfile settings genericStorableFormats with Game = game}
        let origData   = readData origConfig origfile
        let newConfig  = {configFromFile newfile settings genericStorableFormats with Game = game}
        let newData    = readData newConfig newfile
        writeJsonPatchFile outfile (mkExport origConfig origData) (mkExport newConfig newData)
        printfn $"Wrote diff to {outfile}"
        0
    | [| "test" |] ->
        if Map.isEmpty settings.GameDirectoriesForTests then
            printfn "Unable to run tests as GameDirectoriesForTests is empty in your settings.json"
            1
        else
            Map.iter (fun key dir -> Environment.WithGameRomfs key dir) settings.GameDirectoriesForTests
            defaultMainThisAssembly argv
    | [|"help"|]
    | [||] ->
        printfn "SMT Tools command line editor"
        printfn "All commands "
        printfn "Available Commands:"
        printfn "  apply_patch original_file json_patch_file output_file"
        printfn "    Takes a file and a json patch file, and applies that patch to the file, saving the output."
        printfn "  apply_patches game_directory patch_directory output_director"
        printfn "    For all .patch.json files in the patch_directory, find the original file in the extracted game directory and apply the patch, saving the output."
        printfn "  build_json json_file output_file"
        printfn "    Converts a JSON file into a binary file"
        printfn "  diff_files file1 file2 output_json_patch_file"
        printfn "    Takes two files, and creates a JSON Patch of the the changes required to convert file1 into file2."
        printfn "  extract_csv input_file_path output_directory"
        printfn "    Converts data in the file into a series of CSV documents for each table or set of binary data."
        printfn "  extract_json input_file_path output_file_or_directory"
        printfn "    Convert the given binary file into a JSON file"
        printfn "  test"
        printfn "    Run tests. Game directories must be specified in your settings.json GameDirectoriesForTests."
        0
    | e ->
        let args = System.String.Join(" ", e)
        printfn $"Unknown arguments {args}. Use the help command or give no arguments to see list of commands."
        0