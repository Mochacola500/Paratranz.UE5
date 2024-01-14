
namespace LocresLib
{
    public class LocresNamespace : Dictionary<string, LocresString>
    {
        public LocresFile File { get; init; }
        public string Name { get; set; }

        public LocresNamespace(LocresFile file, string name) : this(file, name, 0)
        {

        }

        public LocresNamespace(LocresFile file, string name, int capacity) : base(capacity) 
        {
            File = file;
            Name = name;
        }

        public override string ToString()
        {
            return Name + ":" + base.ToString();
        }
    }
}
