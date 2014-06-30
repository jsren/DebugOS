
namespace DebugOS.PE
{
    public enum Subsystem
    {
        Unknown           = 0,
        Native            = 1,
        WindowsGUI        = 2,
        WindowsCUI        = 3,
        POSIXCUI          = 7,
        WindowsCEGUI      = 9,
        EFI               = 10,
        EFIBootDriver     = 11,
        EFIRuntimeDriver  = 12,
        EFIROM            = 13,
        XBOX              = 14
    }
}
