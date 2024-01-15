# Paratranz.UE5
A client library for converting .locres File.

# Feature
-  Locres string iteration
-  Convert to paratranz `csv`, `json` and `yml`

# Usage

Load
```cs
var locres = LocresFile.Load(file);
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
ParatranzConvert.FromJson(locres, file);
ParatranzConvert.FromJson(key, locres, file);
ParatranzConvert.FromYml(key, locres, files);
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
csvConverter.Import(key, stream, file);
csvConverter.Import(stream, file);
csvConverter.Import(directory, files);
```
