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
                default:
                    return (ns) => "";
            }
        }

        Action<Stream, string[]> GetImportFunction()
        {
            switch (m_Options.SerializeOption)
            {
                case ParatranzSerializeOption.CSV:
                    return FromCSV;
                case ParatranzSerializeOption.Json:
                    return FromJson;
                default:
                    return (stream, files) => { };
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
                default:
                    return ".txt";
            }
        }

        public void Export(string directory)
        {
            var fn = GetExportFunction();
            var extension = GetExtension();

            foreach (var ns in m_LocresFile)
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
            fn.Invoke(stream, files);
        }

        string ToCSV(LocresNamespace locresNamespace)
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

        string ToJson(LocresNamespace locresNamespace)
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

                rows.Add(new(strKey, strVal, ""));
                keyHash.Add(strKey);
            }
            return JsonSerializer.Serialize(rows);
        }

        void FromCSV(Stream stream, params string[] files)
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

            m_LocresFile.Save(stream);
        }

        void FromJson(Stream stream, params string[] files)
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

            m_LocresFile.Save(stream);
        }
    }
}