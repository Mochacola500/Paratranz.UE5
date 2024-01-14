using LocresLib;

namespace Paratranz.UE5
{
    public abstract class ParatranzConverter
    {
        protected readonly LocresFile m_LocresFile;

        public ParatranzConverter(LocresFile locresFile)
        {
            m_LocresFile = locresFile;
        }

        public abstract void Export(string directory);
        public abstract void Import(string key, string inputPath, string outputPath);

        public void Import(string inputPath, string outputPath)
        {
            var key = Path.GetFileNameWithoutExtension(inputPath);
            Import(key, inputPath, outputPath);
        }
    }
}