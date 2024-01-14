using LocresLib;

namespace Paratranz.UE5
{
    public class ParatranzJsonConverter : ParatranzConverter
    {
        public ParatranzJsonConverter(LocresFile locresFile) : base(locresFile) { }

        public override void Export(string directory)
        {
            foreach (var locresNs in m_LocresFile.Values)
            {
                var csv = ParatranzConvert.ToJson(locresNs);
                var path = Path.Combine(directory, locresNs.Name);
                File.WriteAllText(path + ".json", csv);
            }
        }

        public override void Import(string key, Stream stream, string file)
        {
            ParatranzConvert.FromJson(key, m_LocresFile, file);
            m_LocresFile.Save(stream);
        }
    }
}