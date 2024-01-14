# Paratranz.UE5
A client library for converting .locres File.

# Feature
- Locres string iteration
- Convert to paratranz `csv`, `json` and `yml`

# Usage

Load
```cs
var locres = LocresFile.Load(path);
```

Save
```cs
locres.Save(stream);
```

## Use Convert

Export
```cs
ParatranzConvert.ToCSV(locresNs);
ParatranzConvert.ToJson(locresNs);
ParatranzConvert.ToYml(locresNs);
```

Import
```cs
ParatranzConvert.FromCSV(locres, key, path);
ParatranzConvert.FromJson(locres, key, path);
ParatranzConvert.FromYml(locres, key, path);
```

## Use Converter

Export
```cs
var csvConverter = new ParatranzCSVConverter(locres);
csvConverter.Export(directory);
```

Import
```cs
var csvConverter = new ParatranzCSVConverter(locres);
csvConverter.Import(inputPath, outputPath);
```
