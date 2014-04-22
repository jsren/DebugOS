using System;

namespace DebugOS
{
    public enum RegisterType
    {
        GeneralPurpose,
        BasePointer,
        StackPointer,
        InstructionPointer,
        Flags,
        Segment,
        Control,
        Descriptor,
        ModelSpecific,
        Debug,
        Other
    }
}
