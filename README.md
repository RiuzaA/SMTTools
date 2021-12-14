# SMTTools

A command line tool for working with file types used by the Shin Megami Tensei games.

Also includes functionality for patching these files via JSON patches.

## How to use

Commands are formatting like
```
./SMTTools.exe GAME command args...
```
The following game names are supported: SMTSJR, SMTIV, SMTIVA, SMTV, and Unknown.

## Commands

The following commands are supported for the tool:

### help
```help```

Shows list of commands.

### apply_patch
```apply_patch original_file json_patch_file output_file```

Takes a file and a json patch file, and applies that patch to the file, saving the output.

### apply_patches
```apply_patches game_directory patch_directory output_directory```

For all .patch.json files in the patch_directory, find the original file in the extracted game directory and apply the patch, saving the output.

Ex: foo/bar/baz.myfile.patch.json would patch foo/bar/baz.myfile in the game directory, and save is in the output as foo/bar/baz.myfile

### build_json
```build_json json_file output_file```

Converts a JSON file into a binary file.

### diff_files
```diff_files file1 file2 output_json_patch_file```

Takes two files, and creates a JSON Patch of the the changes required to convert file1 into file2.

### extract_csv
```extract_csv input_file_path output_directory```

Converts data in the file into a series of CSV documents for each table or set of binary data.

Not useful for direct modding of files, but makes it easy to compare rows when investigating binary formats.

### extract_json
```extract_json input_file_path output_file_or_directory```

Converts the given binary file into a JSON file

### test (tool development only)
```test```

Run tests. Game directories must be specified in your settings.json GameDirectoriesForTests.

## Supported Binary Types

The following binary data types are supported, as well as games confirmed to use them and be supported by this tool. These may still work for other Atlus games.

### MSG (SMTSJR, SMTIV, SMTIV Apocalypse)

Format for text lines of Shift JIS text

### TBB (SMTSJR)

Format for table data at the base of the file. Support for contained MSG and TBL data.

### TBCR (SMTIV, SMTIV Apocalypse, SMTV)

Format for table data, at the base of the file. Likely replaced TBB format for newer games. Support for contained MSG and TBL data.

### TBL (SMTSJR, SMTIV, SMTIV Apocalypse, SMTV)

Table files containing rows of binary data. Partial support for parsing that binary data for the following cases:

#### Shin Megami Tensei Strange Journey Redux

##### Enemy/NKMBaseTable.tbb
- TBL[0] (60%):  Demon ability data
- TBL[1] (30%):  Additional demon data (including item drops and sprite IDs)

##### Skill/SkillData.tbb
- TBL[0] (95%):  Non-passive skill data
- TBL[1] (80%):  Passive skill data

#### Shin Megami Tensei IV

##### romfs/Battle/NKMSortIndex.tbb
- TBL[0] (100%): Actor/demon race names
- TBL[1] (100%): Actor/demon names

#### Shin Megami Tensei IV Apocalypse

##### romfs/Battle/NKMBaseTable.tbb
- TBL[0] (80%): Demon ability data
- TBL[1] (5%):  Additional demon data (including sprite IDs)

##### romfs/Battle/NKMSortIndex.tbb
- TBL[0] (100%): Actor/demon race names
- TBL[1] (100%): Actor/demon names

##### romfs/Battle/SkillData.tbb
- TBL[0] (30%):  Non-passive skill data

#### Shin Megami Tensei V

##### Project/Content/Blueprints/Gamedata/BinTable/Battle/Skill/SkillData.uasset
- TBL[0]: (50%): Non-passive skill data

##### Project/Content/Blueprints/Gamedata/BinTable/Devil/NKMBaseTable.uasset
- TBL[0]: (25%): Demon stats, affinities, and resistances

### UAsset (SMTV)

Unreal Engine asset types. Will convert to JSON as an UAssetAPI, and will dummy out binary properties, instead processing them and serializing them in the ParsedFromBinary field.
```̀̀̀̀̀
{
	...,
	"Data": {
		"UAsset": {...},
		"ParsedFromBinary": {...}
	}
}
```

## Settings

On first run a settings.json file will be generated in the same directory as the executable. The settings that can be changed are as followed:

### ConvertFromFullWidthInMBM:
```bool```

Full width characters in MBM files will be converted to half width in the JSON file, and converted back to full width on rebuild of the binary table.

### CharacterDictionaryFile
```string```

If not empty, a file is read for converting characters when processing text. This dictionary file is a text file where only rows containing 2 characters are counted, with the first being mapped to the second.

This format matches that used in [Moonbeam](https://github.com/Megaflan/Moonbeam) by Megaflan.

### GameDirectoriesForTests
```Map<string, string```

romfs directories can be given here for each game. This will be accessed when running integration tests. Not needed for standard use.

## Compile Yourself

This project is built in F# for .NET Core 4, compiled using Visual Studio.

In addition, it relies on [UAssetAPI](https://github.com/atenfyr/UAssetAPI) by atenfyr for SMTV file editing. If you are building this yourself, use the following command to clone it into the root of the project:
```
git clone https://github.com/atenfyr/UAssetAPI.git
```
