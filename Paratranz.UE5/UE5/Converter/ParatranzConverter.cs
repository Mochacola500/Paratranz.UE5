using LocresLib;

namespace Paratranz.UE5
{
    public abstract class ParatranzConverter : IParatranzConverter
    {
        protected readonly LocresFile m_LocresFile;

        protected ParatranzConverter(LocresFile locresFile)
        {
            m_LocresFile = locresFile;
        }

        public abstract void Export(string directory);
        public abstract void Import(string key, Stream stream, string file);

        public void Import(Stream stream, string file)
        {
            var key = Path.GetFileNameWithoutExtension(file);
            Import(key, stream, file);
        }

        public void Imports(string directory, params string[] files)
        {
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var path = Path.Combine(directory, name + ".locres");
                using var stream = File.Create(path);
                Import(stream, file);
            }
        }
    }
}