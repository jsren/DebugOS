using System;

namespace DebugOS
{
    [Serializable]
    public class Register : IEquatable<Register>
    {
        public string Name { get; private set; }
        public int Width { get; private set; }
        public RegisterType Type { get; private set; }

        public Register(string Name, int Width)
        {
            this.Name  = Name;
            this.Width = Width;
            this.Type  = RegisterType.GeneralPurpose;
        }
        public Register(string Name, int Width, RegisterType Type)
        {
            this.Name  = Name;
            this.Width = Width;
            this.Type  = Type;
        }

        public bool Equals(Register other)
        {
            return StringComparer.CurrentCultureIgnoreCase.Equals(this.Name, other.Name)
                && this.Width == other.Width
                && this.Type  == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            Register other = obj as Register;
            if (other != null)
            {
                return this.Equals(other);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() + this.Width + Type.GetHashCode();
        }

    }
}
