using System;
using System.IO;

namespace DebugOS.ELF
{
    public class ELFBinary
    {
        public ELFHeader Header { get; private set; }
        public ProgramHeader[] ProgramHeaders { get; private set; }
        public ELF.Section[] Sections { get; private set; }

        public ELFBinary(BinaryReader reader)
        {
            this.Header         = new ELFHeader(reader);
            this.Sections       = new Section[this.Header.SectionTableEntryCount];
            this.ProgramHeaders = new ProgramHeader[this.Header.ProgTableEntryCount];

            long phtoffset = this.Header.ProgramTableOffset;
            long setoffset = this.Header.SectionTableOffset;

            if (phtoffset != 0)
            {
                reader.BaseStream.Seek(phtoffset, SeekOrigin.Begin);

                for (long l = 0; l < this.ProgramHeaders.Length; l++)
                {
                    this.ProgramHeaders[l] = new ProgramHeader(reader, this.Header.Is64Bit);
                }
            }
            if (setoffset != 0)
            {
                reader.BaseStream.Seek(setoffset, SeekOrigin.Begin);

                for (long l = 0; l < this.Sections.Length; l++)
                {
                    this.Sections[l] = new Section(reader, this.Header.Is64Bit);
                }
            }

            // Load section names from string table
            // HOW DO WE FIND WHICH TABLE TO USE?
        }
    }
}
