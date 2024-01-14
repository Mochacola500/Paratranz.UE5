
namespace Paratranz.UE5
{
    internal class JsonObject
    {
        public string key { get; set; }
        public string original { get; set; }
        public string translation { get; set; }

        public JsonObject(string key, string original, string translation)
        {
            this.key = key;
            this.original = original;
            this.translation = translation;
        }
    }
}