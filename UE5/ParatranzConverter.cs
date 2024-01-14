using Csv;
using LocresLib;
using System.Text;
using System.Text.Json;

namespace Paratranz.UE5
{
    internal class JsonObject
    {
        public string key {  get; set; }
        public string original { get; set; }
        public string translation {  get; set; }

        public JsonObject(string key, string original, string translation)
        {
            this.key = key;
            this.original = original;
            this.translation = translation;
        }   
    }

    public class ParatranzConverter
    {
        static readonly string[] g_CsvHeader = new[] { "key", "source", "target" };
        static readonly CsvOptions g_CsvOptions = new() { AllowNewLineInEnclosedFieldValues = true };

        readonly LocresFile m_LocresFile;
        readonly ParatranzConverterOptions m_Options;

        ParatranzConverter(LocresFile file, ParatranzConverterOptions options)
        {
            m_LocresFile = file;
            m_Options = options;
        }

        public static ParatranzConverter Create(LocresFile file, ParatranzConverterOptions? options = null)
        {
            if (options == null)
            {
                options = new();
            }
            return new(file, options);
        }

        Func<LocresNamespace, string> GetExportFunction()
        {
            switch (m_Options.SerializeOption)
            {
                case ParatranzSerializeOption.CSV:
                    return ToCSV;
                case ParatranzSerializeOption.Json:
                    return ToJson;
                case ParatranzSerializeOption.Yml:
                    return ToYml;
                default:
                    return (ns) => "";
            }
        }

        Action<string[]> GetImportFunction()
        {
            switch (m_Options.SerializeOption)
            {
                case ParatranzSerializeOption.CSV:
                    return FromCSV;
                case ParatranzSerializeOption.Json:
                    return FromJson;
                case ParatranzSerializeOption.Yml:
                    return FromYml;
                default:
                    return (files) => { };
            }
        }

        string GetExtension()
        {
            switch (m_Options.SerializeOption)
            {
                case ParatranzSerializeOption.CSV:
                    return ".csv";
                case ParatranzSerializeOption.Json:
                    return ".json";
                case ParatranzSerializeOption.Yml:
                    return ".yml";
                default:
                    return ".txt";
            }
        }

        public void Export(string directory)
        {
            var fn = GetExportFunction();
            var extension = GetExtension();

            foreach (var ns in m_LocresFile.Values)
            {
                if (ns == null)
                {
                    continue;
                }
                var text = fn.Invoke(ns);
                var path = Path.Combine(directory, ns.Name + extension);
                File.WriteAllText(path, text);
            }
        }

        public void Import(Stream stream, params string[] files)
        {
            var fn = GetImportFunction();
            fn.Invoke(files);
            m_LocresFile.Save(stream);
        }

        string ToCSV(LocresNamespace locresNamespace)
        {
            var rows = new List<string[]>();
            var keyHash = new HashSet<string>();

            foreach (var str in locresNamespace.Values)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    throw new Exception("Duplicate key exception.");
                }
                if (string.IsNullOrEmpty(strVal))
                {
                    strVal = "";
                }
                rows.Add(new[] { strKey, strVal, "" });
                keyHash.Add(strKey);
            }

            return CsvWriter.WriteToText(g_CsvHeader, rows);
        }

        string ToJson(LocresNamespace locresNamespace)
        {
            var rows = new List<JsonObject>();
            var keyHash = new HashSet<string>();

            foreach (var str in locresNamespace.Values)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    throw new Exception("Duplicate key exception.");
                }
                if (string.IsNullOrEmpty(strVal))
                {
                    strVal = "";
                }

                rows.Add(new(strKey, strVal, ""));
                keyHash.Add(strKey);
            }
            return JsonSerializer.Serialize(rows);
        }

        string ToYml(LocresNamespace locresNamespace)
        {
            var keyHash = new HashSet<string>();
            var sb = new StringBuilder(1024 * 1024);
            sb.Append("l_").Append(m_Options.CountryTag).AppendLine(":");
            foreach (var str in locresNamespace.Values)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    throw new Exception("Duplicate key exception.");
                }
                if (string.IsNullOrEmpty(strVal))
                {
                    strVal = "";
                }
                sb.Append(strKey).Append(": ").AppendLine(strVal);
                keyHash.Add(strKey);
            }

            return sb.ToString();
        }

        void FromCSV(params string[] files)
        {
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!m_LocresFile.TryGetValue(name, out var ns))
                {
                    throw new Exception($"\"{name}\" is not exist in locres file.\nFile name must be the same as the namespace.");
                }
                if (ns == null)
                {
                    throw new Exception("Invalid locresFile exception.");
                }

                var kvMap = new Dictionary<string, string>();

                using (var sr = new StreamReader(file))
                {
                    foreach (var line in CsvReader.Read(sr, g_CsvOptions))
                    {
                        if (line.ColumnCount < 3)
                        {
                            throw new Exception("Invalid csv file.");
                        }
                        var key = line[0];
                        var target = line[2];

                        kvMap.Add(key, target);
                    }
                }

                foreach (var str in ns.Values)
                {
                    if (!kvMap.TryGetValue(str.Key, out var value))
                    {
                        continue;
                    }
                    if (value == null)
                    {
                        value = "";
                    }
                    str.Value = value.Replace("\\n", "\n");
                }
            }
        }

        void FromJson(params string[] files)
        {
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!m_LocresFile.TryGetValue(name, out var ns))
                {
                    throw new Exception($"\"{name}\" is not exist in locres file.\nFile name must be the same as the namespace.");
                }
                if (ns == null)
                {
                    throw new Exception("Invalid locresFile exception.");
                }

                var jsonStr = File.ReadAllText(file);
                var jsonArray = JsonSerializer.Deserialize<List<JsonObject>>(jsonStr);
                if (jsonArray == null)
                {
                    throw new Exception("Ivalid json type");
                }
                var kvMap = jsonArray.ToDictionary(x => x.key, x => x.translation);

                foreach (var str in ns.Values)
                {
                    if (!kvMap.TryGetValue(str.Key, out var value))
                    {
                        continue;
                    }
                    if (value == null)
                    {
                        value = "";
                    }
                    str.Value = value.Replace("\\n", "\n");
                }
            }
        }

        void FromYml(params string[] files)
        {
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!m_LocresFile.TryGetValue(name, out var ns))
                {
                    throw new Exception($"\"{name}\" is not exist in locres file.\nFile name must be the same as the namespace.");
                }
                if (ns == null)
                {
                    throw new Exception("Invalid locresFile exception.");
                }

                var lines = File.ReadAllLines(file);
                var kvMap = new Dictionary<string, string>();
                int i = 0;

                // Read country tag.
                for (; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }
                    line = line.Trim();
                    if (line.StartsWith("l_") && line.EndsWith(':'))
                    {
                        var fileCountryTag = line.Substring(2, line.Length - 3);
                        if (fileCountryTag != m_Options.CountryTag)
                        {
                            throw new Exception($"Invalid country tag.\nexpected:{m_Options.CountryTag}\nreal:{fileCountryTag}");
                        }
                        break;
                    }
                }

                // Read data.
                for (; i < lines.Length; ++i)
                {
                    var line = lines[i];
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }
                    var result = line.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (result.Length == 0)
                    {
                        continue;
                    }
                    if (result.Length == 1)
                    {
                        throw new Exception($"Invalid context\n{result[0]}");
                    }
                    kvMap.Add(result[0], result[1]);
                }

                foreach (var str in ns.Values)
                {
                    if (!kvMap.TryGetValue(str.Key, out var value))
                    {
                        continue;
                    }
                    if (value == null)
                    {
                        value = "";
                    }
                    str.Value = value.Replace("\\n", "\n");
                }
            }
        }
    }
}