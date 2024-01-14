using System.Text;
using System.Text.Json;
using Csv;
using LocresLib;

namespace Paratranz.UE5
{
    public static class ParatranzConvert
    {
        static readonly string[] g_CsvHeader = new[] { "key", "source", "target" };
        static readonly CsvOptions g_CsvOptions = new() { AllowNewLineInEnclosedFieldValues = true };

        public static string To(ParatranzConvertType type, LocresNamespace locresNs)
        {
            switch (type)
            {
                case ParatranzConvertType.CSV:
                    return ToCSV(locresNs);
                case ParatranzConvertType.Json:
                    return ToJson(locresNs);
                case ParatranzConvertType.Yml:
                    return ToYml(locresNs);
                default:
                    return "";
            }
        }

        public static void From(ParatranzConvertType type, LocresFile locresFile, string key, string path)
        {
            switch (type)
            {
                case ParatranzConvertType.CSV:
                    FromCSV(locresFile, key, path);
                    break;
                case ParatranzConvertType.Json:
                    FromJson(locresFile, key, path);
                    break;
                case ParatranzConvertType.Yml:
                    FromYml(locresFile, key, path);
                    break;
                default:
                    break;
            }
        }

        public static string ToCSV(LocresNamespace locresNs)
        {
            var values = locresNs.Values;
            int capacity = values.Count;
            var rows = new List<string[]>(capacity);
            var keyHash = new HashSet<string>(capacity);

            foreach (var str in values)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    throw new Exception($"Duplicate key exception.\nNamespaceId:{locresNs.Name}\nKey:{strKey}");
                }
                rows.Add(new[] { strKey, strVal, "" });
                keyHash.Add(strKey);
            }

            return CsvWriter.WriteToText(g_CsvHeader, rows);
        }

        public static string ToJson(LocresNamespace locresNs)
        {
            int capacity = locresNs.Count;
            var rows = new List<JsonObject>(capacity);
            var keyHash = new HashSet<string>(capacity);

            foreach (var str in locresNs.Values)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    throw new Exception($"Duplicate key exception.\nNamespaceId:{locresNs.Name}\nKey:{strKey}");
                }
                rows.Add(new(strKey, strVal, ""));
                keyHash.Add(strKey);
            }
            return JsonSerializer.Serialize(rows);
        }

        public static string ToYml(LocresNamespace locresNs)
        {
            int capacity = locresNs.Count;
            var keyHash = new HashSet<string>(capacity);
            var sb = new StringBuilder(1024 * 1024);
            sb.Append("l_").Append(locresNs.File.CountryTag).AppendLine(":");
            foreach (var str in locresNs.Values)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    throw new Exception($"Duplicate key exception.\nNamespaceId:{locresNs.Name}\nKey:{strKey}");
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

        public static void FromCSV(LocresFile locresFile, string key, string path)
        {
            if (!locresFile.TryGetValue(key, out var ns))
            {
                throw new Exception($"key is not exist in locres file.\nKey:{key}\nPath:{path}");
            }
            using var sr = new StreamReader(path);
            foreach (var line in CsvReader.Read(sr, g_CsvOptions))
            {
                if (line.ColumnCount < 3)
                {
                    throw new Exception("Invalid csv file.");
                }
                if (ns.TryGetValue(line[0], out var locresStr))
                {
                    locresStr.Value = line[2].Replace("\\n", "\n");
                }
            }
        }

        public static void FromJson(LocresFile locresFile, string key, string path)
        {
            if (!locresFile.TryGetValue(key, out var ns))
            {
                throw new Exception($"key is not exist in locres file.\nKey:{key}\nPath:{path}");
            }

            var jsonStr = File.ReadAllText(path);
            var jsonArray = JsonSerializer.Deserialize<List<JsonObject>>(jsonStr);
            if (jsonArray == null)
            {
                throw new Exception("Invalid json type");
            }
            foreach (var jsonObject in jsonArray)
            {
                if (ns.TryGetValue(jsonObject.key, out var locresStr))
                {
                    locresStr.Value = jsonObject.translation.Replace("\\n", "\n");
                }
            }
        }

        public static void FromYml(LocresFile locresFile, string key, string path)
        {
            if (!locresFile.TryGetValue(key, out var ns))
            {
                throw new Exception($"key is not exist in locres file.\nKey:{key}\nPath:{path}");
            }

            var lines = File.ReadAllLines(path);
            int i = 0;
            // Read country tag.
            for (; i < lines.Length; ++i)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("l_") && line.EndsWith(':'))
                {
                    break;
                }
            }
            // Read data.
            for (; i < lines.Length; ++i)
            {
                var result = lines[i].Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (result.Length == 0)
                {
                    continue;
                }
                if (result.Length == 1)
                {
                    throw new Exception($"Invalid context\n{result[0]}");
                }
                if (ns.TryGetValue(result[0], out var locresStr))
                {
                    locresStr.Value = result[1].Replace("\\n", "\n");
                }
            }
        }
    }
}