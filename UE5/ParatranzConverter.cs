using Csv;
using LocresLib;
using System.Text.Json;

namespace Paratranz.UE5
{
    internal class JsonObject
    {
        public string key {  get; set; }
        public string original { get; set; }
        public string translation {  get; set; }
    }

    public class ParatranzConverter
    {
        static readonly string[] g_CsvHeader = new[] { "key", "source", "target" };
        static readonly CsvOptions g_CsvOptions = new() { AllowNewLineInEnclosedFieldValues = true };

        readonly LocresFile m_LocresFile;

        public ParatranzConverter(LocresFile file)
        {
            m_LocresFile = file;
        }

        public static string ToCsv(LocresNamespace locresNamespace)
        {
            var rows = new List<string[]>();
            var keyHash = new HashSet<string>();

            foreach (var str in locresNamespace)
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

        public static string ToJson(LocresNamespace locresNamespace)
        {
            var rows = new List<JsonObject>();
            var keyHash = new HashSet<string>();

            foreach (var str in locresNamespace)
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

                rows.Add(new JsonObject { key = strKey, original = strVal, translation = "" });
                keyHash.Add(strKey);
            }

            return JsonSerializer.Serialize(rows);
        }

        public void ExportCsv(string directory)
        {
            foreach (var ns in m_LocresFile)
            {
                if (ns == null)
                {
                    // ERROR
                    continue;
                }
                
                var csv = ToCsv(ns);
                var path = Path.Combine(directory, ns.Name + ".csv");
                File.WriteAllText(path, csv);
            }
        }

        public void ImportCsv(params string[] files)
        {
            var nsMap = m_LocresFile.ToDictionary(x => x.Name, x => x);

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!nsMap.TryGetValue(name, out var ns))
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

                foreach (var str in ns)
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

        public void ExportJson(string directory)
        {
            foreach (var ns in m_LocresFile)
            {
                if (ns == null)
                {
                    // ERROR
                    continue;
                }

                var csv = ToJson(ns);
                var path = Path.Combine(directory, ns.Name + ".json");
                File.WriteAllText(path, csv);
            }
        }

        public void ImportJson(params string[] files)
        {
            var nsMap = m_LocresFile.ToDictionary(x => x.Name, x => x);

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!nsMap.TryGetValue(name, out var ns))
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

                foreach (var str in ns)
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