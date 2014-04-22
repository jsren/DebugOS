
namespace DebugOS
{
    public struct Register
    {
        public string Name { get; private set; }

        public int Width { get; private set; }
        public RegisterType Type { get; private set; }

        public bool CanRead { get; private set; }
        public bool CanWrite { get; private set; }

        public Register(string Name, int Width) : this()
        {
            this.Name  = Name;
            this.Width = Width;
            this.Type  = RegisterType.GeneralPurpose;

            this.CanRead  = true;
            this.CanWrite = true;
        }
        public Register(string Name, int Width, RegisterType Type) : this()
        {
            this.Name  = Name;
            this.Width = Width;
            this.Type  = Type;

            this.CanRead  = true;
            this.CanWrite = true;
        }

        /*public override bool Equals(object obj)
        {
            if (obj is Register)
            {
                Register r = (Register)obj;

                return (r.CanRead == this.CanRead && r.CanWrite == this.CanWrite && 
                    r.Name == this.Name && r.Type == this.Type && r.Width == this.Width);
            }
            else return base.Equals(obj);
        }*/
    }
}
