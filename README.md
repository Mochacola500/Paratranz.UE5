# Paratranz.UE5  

A client library for converting .locres File.

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/4e556876a3c54d5e8f2d2857c4f43894)][codacy]&nbsp;
[![GitHub license](https://img.shields.io/github/license/cotes2020/jekyll-theme-chirpy.svg)][license]&nbsp;

# Feature
-  Locres string iteration
-  Convert to paratranz `csv`, `json` and `yml`

# Requirement
-  `Csv`

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

[codacy]: https://app.codacy.com/gh/Mochacola500/Paratranz.UE5/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade
[license]: https://github.com/Mochacola500/Paratranz.UE5/blob/master/LICENSE
