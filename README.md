# Paratranz.UE5
A client library for converting .locres File.

# Feature
- Locres string enumeration
- Convert to `csv`, `json` and `yml`

# Usage

Create a instance

```cs
using var fs = File.Open(path);
var locres = LocresFile.Load(fs);
var options = new ParatranzConverterOptions()
{
    SerializeOption = ParatranzSerializeOption.Yml,
    CountryTag = "english"
};
var converter = ParatranzConverter.Create(locres, options);
```

Export

```cs
converter.Export(path);
```

Import

```cs
converter.Import(fs, files);
```
