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
        public abstract void Import(string key, string newFileName, string file);

        public void Import(string newFileName, string file)
        {
            var key = Path.GetFileNameWithoutExtension(file);
            Import(key, newFileName, file);
        }

        public void Import(string newFileName, params string[] files)
        {
            foreach (var inputFile in files)
            {
                Import(newFileName, inputFile);
            }
        }
    }
}