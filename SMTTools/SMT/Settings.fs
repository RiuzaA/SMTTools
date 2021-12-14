module SMT.Settings

type Settings =
    { ConvertFromFullWidthInMBM: bool
      CharacterDictionaryFile:   string
      GameDirectoriesForTests:   Map<string, string> }

let defaultSettings =
    { ConvertFromFullWidthInMBM  = false
      CharacterDictionaryFile    = ""
      GameDirectoriesForTests    = Map.empty }