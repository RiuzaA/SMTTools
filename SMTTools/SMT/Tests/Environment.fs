module SMT.Tests.Environment

open Fuchu
open System.Collections.Generic
open System.IO

open SMT.AllGames
open SMT.Settings
open SMT.Types
open SMT.Utils

type Environment private () =
    static member val GameRomfs: Map<string, string> = Map.empty with get, set

    static member WithGameRomfs (gameID: string) (gameDir: string) = Environment.GameRomfs <- Map.add gameID gameDir Environment.GameRomfs

module Utils =
    let storableFormats = [ storableFormat <| SMT.Formats.TBB.TBBStorer()
                            storableFormat <| SMT.Formats.TBCR.TBCRStorer()
                            storableFormat <| SMT.Formats.MSG.MSGStorer() ]

    let encodeDecodeFileTest game (filename: string) =
        let path = Map.tryFind game.ID Environment.GameRomfs
                |> Option.defaultValue ""
                |> (+) ("/" + filename)
        let config = {configFromFile path defaultSettings storableFormats with Game = game}
        use reader = new BinaryReader(File.OpenRead(path))
        let bytes = reader.ReadBytes (int reader.BaseStream.Length)
        reader.BaseStream.Position <- 0L
        let storer = findFirstReadableFormat config reader |> Option.get
        let data = iRead config storer reader
        use outputMem = new MemoryStream()
        use writer = new BinaryWriter(outputMem)     
        iWrite config storer data writer
        let finalBytes = outputMem.ToArray()
        Assert.Equal("bytes are the same with no changes", bytes, finalBytes)

    let writeToMemoryConfig game fileName (memStreamDict: Dictionary<string, MemoryStream>) =
        let def = configFromFile fileName defaultSettings genericStorableFormats
        { def with Game = game
                   Context = {def.Context with GetFileWriter = fun fileName ->
                                                                   let memStream =
                                                                       if memStreamDict.ContainsKey fileName then
                                                                           memStreamDict.[fileName]
                                                                        else
                                                                            let memStream = new MemoryStream()
                                                                            memStreamDict.[fileName] <- memStream
                                                                            memStream
                                                                   new BinaryWriter(memStream)}}