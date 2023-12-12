# Paratranz.UE5
A client library for converting .locres File.

# Feature

- CSV Convert
- Json Convert

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

    or...

    converter.ImportJson(fs, files);
}
```
