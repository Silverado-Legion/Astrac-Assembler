namespace Astrac.Assemblers
{
    internal class IsaSettings
    {
        public string Version { get; set; }
        public string OutType {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public byte InstWidth { get; private set; }

        public Dictionary<string, byte> Registers { get; set; } = new Dictionary<string, byte>();
        public Dictionary<string, Instruction> Instructions { get; set; } = new Dictionary<string, Instruction>();

        public IsaSettings(string ver = "", string outType = "", string name = "", string desc = "")
        {
            Version = ver;
            OutType = outType;
            Name = name;
            Description = desc;
        }

        public void SetWidth()
        {
            if (Instructions.Any())
            {
                string FirstInst = Instructions.First().Value.BitField[0];
                InstWidth = (byte)FirstInst.Length;
            }
        }
    }

    internal struct Instruction
    {
        public string Name;
        public List<string> BitField;
        public List<string> Parameters;

        public Instruction()
        {
            Name = "";
            BitField = new List<string>();
            Parameters = new List<string>();
        }
    }
}