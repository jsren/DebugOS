using System;
using System.IO;

namespace DebugOS.ELF
{
    public sealed class ELFHeader
    {
        public bool Is64Bit { get; private set; }
        public bool IsBigEndian { get; private set; }
        public Version Version { get; private set; }
        public ABI TargetOS { get; private set; }
        public Version TargetOSVersion { get; private set; }
        public FileType Type { get; private set; }
        public MachineType TargetMachine { get; private set; }
        public ulong EntryPoint { get; private set; }
        public long ProgramTableOffset { get; private set; }
        public long SectionTableOffset { get; private set; }
        public uint Flags { get; private set; }
        public int HeaderSize { get; private set; }
        public int ProgTableEntrySize { get; private set; }
        public int ProgTableEntryCount { get; private set; }
        public int SectionTableEntrySize { get; private set; }
        public int SectionTableEntryCount { get; private set; }
        public int SectionNameIndex { get; private set; }


        public ELFHeader(BinaryReader reader)
        {
            byte Class, Encoding;

            // Validate the header magic
            if (reader.ReadByte() != 0x7F || reader.ReadByte() != 'E' ||
                reader.ReadByte() != 'L' || reader.ReadByte() != 'F')
            {
                throw new BadImageFormatException("Invalid ELF magic");
            }

            Class    = reader.ReadByte();
            Encoding = reader.ReadByte();

            if (Class == 0 || Encoding == 0) {
                throw new BadImageFormatException("Invalid class or data encoding");
            }

            // Read the ELF identification
            this.Is64Bit         = (Class == 2);
            this.IsBigEndian     = (Encoding == 2);
            this.Version         = new Version(reader.ReadByte(), 0);
            this.TargetOS        = (ABI)reader.ReadByte();
            this.TargetOSVersion = new Version(reader.ReadByte(), 0);

            reader.ReadBytes(7); // Skip padding bytes

            // Read the rest of the ELF header
            this.Type          = (FileType)reader.ReadUInt16();
            this.TargetMachine = (MachineType)reader.ReadUInt16();

            reader.ReadUInt32(); // Skip redundant field

            this.EntryPoint         = Is64Bit ? reader.ReadUInt64() : reader.ReadUInt32();
            this.ProgramTableOffset = Is64Bit ? reader.ReadInt64()  : reader.ReadUInt32();
            this.SectionTableOffset = Is64Bit ? reader.ReadInt64()  : reader.ReadUInt32();

            this.Flags                  = reader.ReadUInt32();
            this.HeaderSize             = reader.ReadUInt16();
            this.ProgTableEntrySize     = reader.ReadUInt16();
            this.ProgTableEntryCount    = reader.ReadUInt16();
            this.SectionTableEntrySize  = reader.ReadUInt16();
            this.SectionTableEntryCount = reader.ReadUInt16();
            this.SectionNameIndex       = reader.ReadUInt16();

            // A little controversial - if they've overflown, throw an exception
            if (this.ProgramTableOffset < 0 || this.SectionTableOffset < 0)
            {
                throw new BadImageFormatException("Image offset values are too large to be "+
                    "supported by this implementation");
            }
        }
    }
}
