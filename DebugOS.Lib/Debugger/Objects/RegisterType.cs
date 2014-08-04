using System;

namespace DebugOS
{
    public enum RegisterType
    {
        GeneralPurpose,
        FramePointer,
        StackPointer,
        InstructionPointer,
        ReturnAddress,
        Flags,
        Segment,
        Control,
        Extended,
        ModelSpecific,
        Debug,
        Other
    }
}
