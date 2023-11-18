using Csv;
using LocresLib;

namespace Paratranz.UE5
{
    public static class CsvConverter
    {
        static readonly string[] g_ParadoxCsvHeader = new[] { "key", "source", "target" };
        static readonly CsvOptions g_CsvOptions = new() { AllowNewLineInEnclosedFieldValues = true };

        public static void Import(string locresFilePath, params string[] filePaths)
        {
            using var fs = File.OpenRead(locresFilePath);
            var locresFile = new LocresFile();
            locresFile.Load(fs);
            Import(locresFile, filePaths);
        }

        public static void Export(string locresFilePath, string directory)
        {
            using var fs = File.OpenRead(locresFilePath);
            var locresFile = new LocresFile();
            locresFile.Load(fs);
            Export(locresFile, directory);
        }

        public static void Import(LocresFile locresFile, params string[] filePaths)
        {
            var nsMap = new Dictionary<string, LocresNamespace>();
            foreach (var ns in locresFile)
            {
                nsMap.Add(ns.Name, ns);
            }
            foreach (var filePath in filePaths)
            {
                if (!nsMap.TryGetValue(filePath, out var ns))
                {
                    // WARNING
                    continue;
                }
                if (ns == null)
                {
                    // ERROR
                    continue;
                }
                ImportNamespace(ns, filePath);
            }
        }

        public static void Export(LocresFile locresFile, string directory)
        {
            foreach (var ns in locresFile)
            {
                if (ns == null)
                {
                    // ERROR
                    continue;
                }
                ExportNamespace(ns, directory);
            }
        }

        public static void ImportNamespace(LocresNamespace locresNamespace, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName != locresNamespace.Name)
            {
                // WARNING
                return;
            }
            using (var fs = File.OpenRead(filePath))
            {
                ImportNamespace(locresNamespace, fs);
            }
        }

        public static void ImportNamespace(LocresNamespace locresNamespace, Stream stream)
        {
            var kvMap = new Dictionary<string, string>();
            using (var sr = new StreamReader(stream))
            {
                foreach (var line in CsvReader.Read(sr, g_CsvOptions))
                {
                    if (line.ColumnCount < 3)
                    {
                        // ERROR
                        continue;
                    }
                    var key = line[0];
                    //var source = line[1];
                    var taret = line[2];

                    if (!kvMap.TryAdd(key, taret))
                    {
                        // WARNING
                    }
                }
            }

            foreach (var str in locresNamespace)
            {
                if (!kvMap.TryGetValue(str.Key, out var value))
                {
                    // WARNING
                    continue;
                }
                if (value == null)
                {
                    value = "";
                }
                str.Value = value;
            }
        }

        public static void ExportNamespace(LocresNamespace locresNamespace, string directory)
        {
            var csv = ToCsv(locresNamespace);
            var filePath = Path.Combine(directory, locresNamespace.Name + ".csv");
            File.WriteAllText(filePath, csv);
        }

        public static string ToCsv(LocresNamespace locresNamespace)
        {
            var rows = new List<string[]>();
            // 키값이 고유한지 확인해야한다.
            var keyHash = new HashSet<string>();
            foreach (var str in locresNamespace)
            {
                var strKey = str.Key;
                var strVal = str.Value;
                if (keyHash.Contains(strKey))
                {
                    // ERROR
                    continue;
                }
                if (string.IsNullOrEmpty(strVal))
                {
                    strVal = "";
                }
                rows.Add(new[] { strKey, strVal, "" });
                keyHash.Add(strKey);
            }

            return CsvWriter.WriteToText(g_ParadoxCsvHeader, rows);
        }
    }
}