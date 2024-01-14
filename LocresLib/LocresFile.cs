using System.Text;
using LocresLib.IO;

namespace LocresLib
{
    public class LocresFile : Dictionary<string, LocresNamespace>
    {
        static byte[] g_LocresMagic = new byte[]
        {
            0x0E, 0x14, 0x74, 0x75, 0x67, 0x4A, 0x03, 0xFC, 0x4A, 0x15, 0x90, 0x9D, 0xC3, 0x37, 0x7F, 0x1B
        };

        LocresFile()
        {

        }

        public static LocresFile Load(string path)
        {
            using var fs = File.OpenRead(path);
            return Load(fs);
        }

        public static LocresFile Load(Stream stream)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable.");

            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable.");

            using var reader = new BinaryReader(stream);
            var version = LocresVersion.Legacy;

            byte[] magic = reader.ReadBytes(0x10);

            if (g_LocresMagic.SequenceEqual(magic))
            {
                version = (LocresVersion)reader.ReadByte();
            }
            else
            {
                version = LocresVersion.Legacy;
                reader.BaseStream.Position = 0;
            }

            string[]? localizedStringArray = null;

            if (version >= LocresVersion.Compact)
            {
                long localizedStringArrayOffset = reader.ReadInt64();
                long tempOffset = reader.BaseStream.Position;
                reader.BaseStream.Position = localizedStringArrayOffset;

                int localizedStringCount = reader.ReadInt32();
                localizedStringArray = new string[localizedStringCount];

                if (version >= LocresVersion.Optimized)
                {
                    for (int i = 0; i < localizedStringCount; i++)
                    {
                        localizedStringArray[i] = reader.ReadUnrealString();
                        reader.ReadInt32(); //refCount
                    }
                }
                else
                {
                    for (int i = 0; i < localizedStringCount; i++)
                    {
                        localizedStringArray[i] = reader.ReadUnrealString();
                    }
                }

                reader.BaseStream.Position = tempOffset;
            }

            if (localizedStringArray == null)
            {
                localizedStringArray = Array.Empty<string>();
            }

            if (version >= LocresVersion.Optimized)
                reader.ReadInt32(); // entriesCount

            int namespaceCount = reader.ReadInt32();
            var file = new LocresFile();

            for (int i = 0; i < namespaceCount; ++i)
            {
                if (version >= LocresVersion.Optimized)
                    reader.ReadUInt32(); // namespaceKeyHash

                string namespaceKey = reader.ReadUnrealString();

                int keyCount = reader.ReadInt32();

                var ns = new LocresNamespace(namespaceKey);

                for (int j = 0; j < keyCount; j++)
                {
                    if (version >= LocresVersion.Optimized)
                    {
                        var stringKeyHash = reader.ReadUInt32();
                    }

                    string stringKey = reader.ReadUnrealString();
                    uint sourceStringHash = reader.ReadUInt32();

                    string localizedString;

                    if (version >= LocresVersion.Compact)
                    {
                        int stringIndex = reader.ReadInt32();
                        localizedString = localizedStringArray[stringIndex];
                    }
                    else
                    {
                        localizedString = reader.ReadUnrealString();
                    }

                    ns.Add(stringKey, new(stringKey, localizedString, sourceStringHash));
                }
                file.Add(ns.Name, ns);
            }

            return file;
        }

        public void Save(Stream stream, LocresVersion outputVersion = LocresVersion.Compact)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable.");

            if (!stream.CanWrite)
                throw new ArgumentException("Stream must be writeable.");

            using (BinaryWriter w = new BinaryWriter(stream))
            {
                if (outputVersion == LocresVersion.Legacy)
                {
                    SaveLegacy(w);
                    return;
                }

                w.Write(g_LocresMagic);                  // byte LOCRES_MAGIC[16]
                w.Write((byte)outputVersion);           // byte version
                long arrayOffset = w.BaseStream.Position;
                w.Write((long)0);                       // long localizedStringArrayOffset

                if (outputVersion >= LocresVersion.Optimized)
                    w.Write(0); // int localizedStringEntryCount

                w.Write(Count); // int namespaceCount

                var stringTable = new List<StringTableEntry>();
                int localizedStringEntryCount = 0;

                foreach (var localizationNamespace in Values)
                {
                    if (outputVersion == LocresVersion.Optimized_CityHash64_UTF16)
                        w.Write(CityHash64_utf16_to_uint32(localizationNamespace.Name));
                    else if (outputVersion >= LocresVersion.Optimized)
                        w.Write(Crc.StrCrc32(localizationNamespace.Name));

                    w.WriteUnrealString(localizationNamespace.Name);
                    w.Write(localizationNamespace.Count); // int localizaedStringCounnt

                    foreach (var localizedString in localizationNamespace.Values)
                    {
                        if (outputVersion == LocresVersion.Optimized_CityHash64_UTF16)
                            w.Write(CityHash64_utf16_to_uint32(localizedString.Key));
                        else if (outputVersion == LocresVersion.Optimized)
                            w.Write(Crc.StrCrc32(localizedString.Key));

                        w.WriteUnrealString(localizedString.Key);
                        w.Write(localizedString.SourceStringHash);

                        int stringTableIndex = stringTable.FindIndex(x => x.Text == localizedString.Value);

                        if (stringTableIndex == -1)
                        {
                            stringTableIndex = stringTable.Count;
                            stringTable.Add(new(localizedString.Value, 1));
                        }
                        else
                        {
                            stringTable[stringTableIndex].RefCount += 1;
                        }

                        w.Write(stringTableIndex);
                        localizedStringEntryCount += 1;
                    }
                }

                long stringTableOffset = w.BaseStream.Position;

                w.Write(stringTable.Count);

                if (outputVersion >= LocresVersion.Optimized)
                {
                    foreach (var entry in stringTable)
                    {
                        w.WriteUnrealString(entry.Text);
                        w.Write(entry.RefCount);
                    }
                }
                else
                {
                    foreach (var entry in stringTable)
                    {
                        w.WriteUnrealString(entry.Text);
                    }
                }

                w.BaseStream.Position = arrayOffset;
                w.Write(stringTableOffset); // long localizedStringArrayOffset

                if (outputVersion >= LocresVersion.Optimized)
                    w.Write(localizedStringEntryCount);

                stream.Seek(0, SeekOrigin.End);
            }
        }

        void SaveLegacy(BinaryWriter w)
        {
            w.Write(Count); // int namespaceCount

            foreach (var localizationNamespace in Values)
            {
                w.WriteUnrealString(localizationNamespace.Name, forceUnicode: true);
                w.Write(localizationNamespace.Count);

                foreach (var localizedString in localizationNamespace.Values)
                {
                    w.WriteUnrealString(localizedString.Key);
                    w.Write(localizedString.SourceStringHash);
                    w.WriteUnrealString(localizedString.Value);
                }
            }
        }

        /// <summary>
        ///     Encode string with UTF-16-LE, calculate CityHash64 and get uint32 hash of cityhash.
        ///     <br/>
        ///     uint64 to uint32 hash function: https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Templates/TypeHash.h#L81
        /// </summary>
        /// <param name="s">Input string</param>
        /// <returns>uint32 hash of CityHash64 hash of input string</returns>
        static uint CityHash64_utf16_to_uint32(string s)
        {
            if (s.Length == 0)
                return 0;

            byte[] b = Encoding.Unicode.GetBytes(s);
            ulong h = CityHash.CityHash64(b);
            uint r = (uint)h + ((uint)(h >> 32) * 23);
            return r;
        }

        class StringTableEntry
        {
            public string Text { get; set; }
            public int RefCount { get; set; }

            public StringTableEntry(string text, int refCount)
            {
                Text = text;
                RefCount = refCount;
            }   
        }
    }
}