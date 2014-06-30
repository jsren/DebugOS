
namespace DebugOS.PE
{
    public struct DataDirectory
    {
        public uint VirtualAddress { get; private set; }
        public uint Size           { get; private set; }

        public DataDirectory(ulong value) : this()
        {
            VirtualAddress = (uint)(value >> 0);
            Size           = (uint)(value >> 32);
        }
    }
}
