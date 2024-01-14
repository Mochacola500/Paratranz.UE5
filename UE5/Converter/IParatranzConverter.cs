
namespace Paratranz.UE5
{
    public interface IParatranzConverter
    {
        void Export(string directory);
        void Import(string key, Stream stream, string file);
        void Import(Stream stream, string file);
        void Imports(string directory, params string[] files);
    }
}