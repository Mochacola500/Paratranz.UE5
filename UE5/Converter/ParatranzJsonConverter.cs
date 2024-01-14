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

        public override void Import(string key, string newFileName, string file)
        {
            ParatranzConvert.FromJson(m_LocresFile, key, file);
            using var stream = File.Create(newFileName);
            m_LocresFile.Save(stream);
        }
    }
}