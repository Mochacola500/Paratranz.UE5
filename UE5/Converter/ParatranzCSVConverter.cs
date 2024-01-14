using LocresLib;

namespace Paratranz.UE5
{
    public class ParatranzCSVConverter : ParatranzConverter
    {
        public ParatranzCSVConverter(LocresFile locresFile) : base(locresFile) { }

        public override void Export(string directory)
        {
            foreach (var locresNs in m_LocresFile.Values)
            {
                var csv = ParatranzConvert.ToCSV(locresNs);
                var path = Path.Combine(directory, locresNs.Name);
                File.WriteAllText(path + ".csv", csv);
            }
        }

        public override void Import(string key, string newFileName, string file)
        {
            ParatranzConvert.FromCSV(m_LocresFile, key, file);
            using var stream = File.Create(newFileName);
            m_LocresFile.Save(stream);
        }
    }
}