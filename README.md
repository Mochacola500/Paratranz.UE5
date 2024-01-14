# Paratranz.UE5
A client library for converting .locres File.

# Feature
- Locres string iteration
- Convert to paratranz `csv`, `json` and `yml`

# Usage

Create locresFile
```cs
var locres = LocresFile.Load(path);
```

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
