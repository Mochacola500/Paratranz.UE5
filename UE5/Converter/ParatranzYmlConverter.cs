using LocresLib;

namespace Paratranz.UE5
{
    public class ParatranzYmlConverter : ParatranzConverter
    {
        public ParatranzYmlConverter(LocresFile locresFile) : base(locresFile) { }

        public override void Export(string directory)
        {
            foreach (var locresNs in m_LocresFile.Values)
            {
                var csv = ParatranzConvert.ToYml(locresNs);
                var path = Path.Combine(directory, locresNs.Name);
                File.WriteAllText(path + ".yml", csv);
            }
        }

        public override void Import(string key, string newFileName, string file)
        {
            ParatranzConvert.FromYml(m_LocresFile, key, file);
            using var stream = File.Create(newFileName);
            m_LocresFile.Save(stream);
        }
    }
}