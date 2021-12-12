module SMT.Formats.UAsset

open System
open System.IO
open UAssetAPI
open UAssetAPI.PropertyTypes

open SMT.Utils
open SMT.Types

type ExtendedUAsset =
    { UAsset: UAsset
      ParsedFromBinary: Map<string, obj> }

type DummyPropData() =
    inherit PropertyData(FName.FromString "DummyPropData")

type UAssetStorer() =
    let processBin config (bytes: byte array) =
        let reader = new BinaryReader(new MemoryStream(bytes))
        match findFirstReadableFormat config reader with
        | None ->
            failwith "No format found for parsing file"
        | Some storer ->
            iRead config storer reader

    let forBinaryExports (asset: UAsset) f =
        for exp in asset.Exports do
            match exp with
            | :? NormalExport as export ->
                for prop in export.Data do
                    match prop with
                    | :? ArrayPropertyData as propdata ->
                        if propdata.ArrayType.Value.Value = "ByteProperty" then
                            f propdata
                        ()
                    | _ -> ()
                ()
            | _ -> ()

    interface IHeader with
        member self.IsHeaderMatching config reader = isHeaderBytes [| 0xC1uy; 0x83uy; 0x2Auy; 0x9Euy; 0xF9uy; 0xFFuy; 0xFFuy; 0xFFuy |] reader
    interface IStorable<ExtendedUAsset> with
        member self.Read config reader =
            let asset = new UAsset(UE4Version.VER_UE4_23)
            let memStream = new MemoryStream()
            reader.BaseStream.CopyTo memStream
            let dataFile = Path.ChangeExtension(config.Context.FilePath, "uexp")
            if File.Exists dataFile then
                use dataStream = File.OpenRead(dataFile)
                memStream.Seek(0L, SeekOrigin.End) |> ignore
                dataStream.CopyTo memStream
                asset.UseSeparateBulkDataFiles <- true
            memStream.Seek(0L, SeekOrigin.Begin) |> ignore
            asset.Read(new AssetBinaryReader(memStream, asset))

            let mutable parsedFromBin = Map.empty
            forBinaryExports asset <| fun propdata ->
                let parseByte: PropertyData -> byte = function
                    | :? BytePropertyData as p -> byte p.Value
                    | p -> failwith $"Expected BytePropertyData, got {p.GetType().ToString()} instead"
                let bytes = Array.map parseByte propdata.Value
                propdata.Value <- [| DummyPropData() |]
                parsedFromBin <- Map.add (propdata.Name.ToString()) (processBin config bytes) parsedFromBin
            { UAsset = asset; ParsedFromBinary = parsedFromBin }

        member self.Write config data writer =
            forBinaryExports data.UAsset <| fun propdata ->
                if propdata.Value.Length = 1 && propdata.Value.[0].Name.Value.Value = "DummyPropData" then
                    let name = propdata.Name.ToString()
                    match Map.tryFind name data.ParsedFromBinary with
                    | None -> failwith $"Property `{name}` had no entry in ParsedFromBinary, despite being dummied-out"
                    | Some parsedObj ->
                        let memStream = new MemoryStream()
                        let writer = new BinaryWriter(memStream)
                        let storer = config.Game.Storers.Get<obj IStorable> (parsedObj.GetType())
                        storer.Write config parsedObj writer
                        let mkProp i b = 
                            let prop = new BytePropertyData()
                            prop.ByteType <- BytePropertyType.Byte
                            prop.Name     <- new FName(i.ToString(), Int32.MinValue)
                            prop.Value    <- int b
                            prop :> PropertyData
                        propdata.Value <- Array.mapi mkProp <| memStream.ToArray()
            let memStream = data.UAsset.WriteData()
            if data.UAsset.UseSeparateBulkDataFiles && data.UAsset.Exports.Count > 0 then
                let breakPoint = data.UAsset.Exports.[0].SerialOffset
                let copyPart (output: BinaryWriter) startPos len =
                    ignore <| memStream.Seek(startPos, SeekOrigin.Begin)
                    let memReader = new BinaryReader(memStream)
                    output.Write(memReader.ReadBytes len)
                
                copyPart writer 0L <| int breakPoint
                use uexpWriter = config.Context.GetFileWriter <| Path.ChangeExtension(config.Context.FilePath, "uexp")
                copyPart uexpWriter breakPoint <| int (memStream.Length - breakPoint)
            else
                writer.Write(memStream.ToArray())