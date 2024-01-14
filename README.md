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

Export
```cs
ParatranzConvert.To(convertType, locresNs);
ParatranzConvert.ToCSV(locresNs);
ParatranzConvert.ToJson(locresNs);
ParatranzConvert.ToYml(locresNs);
```
Import
```cs
ParatranzConvert.From(convertType, locres, key, path);
ParatranzConvert.FromCSV(locres, key, path);
ParatranzConvert.FromJson(locres, key, path);
ParatranzConvert.FromYml(locres, key, path);
```
