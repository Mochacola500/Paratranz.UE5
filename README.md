# Paratranz.UE5
A client library for converting .locres File.

# Feature
- Locres string enumeration
- Convert to `json` or `csv`

# Usage

Create a instance

```cs
var locres = new LocresFile();
using var fs = File.Open(filePath);
locres.Load(fs);
var converter = new ParatranzConverter(locres);
```

Export

```cs
converter.ExportCsv(savePath);
converter.ExportJson(savePath);
```

Import

```cs
using (var fs = File.Create(newFilePath))
{
    var files = Directory.GetFiles(dataDirectory);
    converter.ImportCsv(fs, files);
    converter.ImportJson(fs, files);
}
```
