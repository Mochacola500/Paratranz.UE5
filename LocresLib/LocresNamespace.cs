
namespace LocresLib
{
    public class LocresNamespace : Dictionary<string, LocresString>
    {
        public string Name { get; set; }

        public LocresNamespace(string name) : this(name, 0)
        {

        }

        public LocresNamespace(string name, int capacity) : base(capacity) 
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name + ":" + base.ToString();
        }
    }
}
