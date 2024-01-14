# Paratranz.UE5
A client library for converting .locres File.

# Feature
- Locres string enumeration
- Convert to `json` or `csv`

# Usage

Create a instance

```cs
using var fs = File.Open(path);
var locres = LocresFile.Load(fs);
var options = new ParatranzConverterOptions();
var converter = ParatranzConverter.Create(locres, options);
```

Export

```cs
converter.Export(path);
```

Import

```cs
var files = Directory.GetFiles(dir);
using var fs = File.Create(path);
converter.Import(fs, files);
```