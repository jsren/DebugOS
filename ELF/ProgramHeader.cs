using System;
using System.IO;

namespace DebugOS.ELF
{
    public sealed class ProgramHeader
    {
        public HeaderType Type { get; private set; }
        public SegmentFlags SegmentFlags { get; private set; }
        public long SegmentOffset { get; private set; }
        public ulong VirtualAddress { get; private set; }
        public long SegmentFileSize { get; private set; }
        public long SegmentLoadedSize { get; private set; }
        public long Alignment { get; private set; }

        public ProgramHeader(BinaryReader reader, bool Is64Bit)
        {
            this.Type = (HeaderType)reader.ReadUInt32();

            // 64-bit reads flags here
            if (Is64Bit) { this.SegmentFlags = (SegmentFlags)reader.ReadUInt32(); }

            this.SegmentOffset  = Is64Bit ? reader.ReadInt64()  : reader.ReadUInt32();
            this.VirtualAddress = Is64Bit ? reader.ReadUInt64() : reader.ReadUInt32();

            // Skip physical address - reserved
            if (Is64Bit) reader.ReadUInt64(); else reader.ReadUInt32();

            this.SegmentFileSize   = Is64Bit ? reader.ReadInt64() : reader.ReadUInt32();
            this.SegmentLoadedSize = Is64Bit ? reader.ReadInt64() : reader.ReadUInt32();

            // 32-bit reads flags here
            if (!Is64Bit) { this.SegmentFlags = (SegmentFlags)reader.ReadUInt32(); }

            this.Alignment = Is64Bit ? reader.ReadInt64() : reader.ReadUInt32();

            if (this.SegmentOffset < 0 || this.SegmentFileSize < 0 ||
                this.SegmentLoadedSize < 0 || this.Alignment < 0)
            {
                throw new BadImageFormatException("Program header values are too large to be " +
                    "supported by this implementation");
            }
        }
    }
}
