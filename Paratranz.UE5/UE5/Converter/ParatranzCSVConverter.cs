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

        public override void Import(string key, Stream stream, string file)
        {
            ParatranzConvert.FromCSV(key, m_LocresFile, file);
            m_LocresFile.Save(stream);
        }
    }
}