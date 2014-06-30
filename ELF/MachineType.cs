
namespace DebugOS.ELF
{
    public enum MachineType
    {
        Unknown    = 0x0,
        WE32100    = 0x1,
        SPARC      = 0x2,
        x86        = 0x3,
        Moto68000  = 0x4,
        Moto88000  = 0x5,
        MIPS       = 0x8,
        PowerPC    = 0x14,
        ARM        = 0x28,
        SuperH     = 0x2A,
        IA64       = 0x32,
        AMD64      = 0x3E,
        AArch64    = 0xB7,
    }
}
