
namespace DebugOS
{
    public struct Address
    {
        public long Value { get; private set; }
        public AddressType Type { get; private set; }

        public Address(long value) : this()
        {
            this.Type  = AddressType.Physical;
            this.Value = value;
        }

        public Address(AddressType type, long value) : this()
        {
            this.Type  = type;
            this.Value = value;
        }

        public static bool operator ==(Address addr1, Address addr2) {
            return addr1.Type == addr2.Type && addr1.Value == addr2.Value;
        }
        public static bool operator !=(Address addr1, Address addr2) {
            return addr1.Type != addr2.Type || addr1.Value != addr2.Value;
        }

        public static implicit operator Address(long value) {
            return new Address(value);
        }
    }
}
