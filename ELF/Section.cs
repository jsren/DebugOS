/* Section.cs - Logic (c) James S Renwick 2014
 * -------------------------------------------
 * Version 1.0.0
 */

using System;
using System.IO;

namespace DebugOS.ELF
{
    /// <summary>
    /// Provides information on a section within an ELF binary.
    /// </summary>
    public sealed class Section
    {
        /// <summary>
        /// The name of the section. This must be loaded from the string
        /// table using the SetName method.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Holds the string table index of the section name.
        /// </summary>
        public long NameIndex { get; private set; }
        /// <summary>
        /// The type of data the section holds.
        /// </summary>
        public SectionType Type { get; private set; }
        /// <summary>
        /// Section access attributes.
        /// </summary>
        public SectionFlags Flags { get; private set; }
        /// <summary>
        /// If the section will appear in the memory image of a process, gives the address at which 
        /// the section’s ﬁrst byte should reside. Otherwise zero.
        /// </summary>
        public ulong LoadAddress { get; private set; }
        /// <summary>
        /// Gives the byte offset from the beginning of the ﬁle to the ﬁrst byte in the section
        /// </summary>
        public long FileOffset { get; private set; }
        /// <summary>
        /// Gives the section’s size in bytes. 
        /// 
        /// The section occupies the given number of bytes in the ﬁle unless the section contains 
        /// uninitialised data. Such a section may have a non-zero size but occupies no space in the ﬁle.
        /// </summary>
        public long Size { get; private set; }
        /// <summary>
        /// The index to a linked section header, whose interpretation depends on the section type.
        /// </summary>
        public int LinkedSectionIndex { get; private set; }
        /// <summary>
        /// Section type-dependent auxiliary information.
        /// 
        /// If the section contains relocation information, this field holds the index
        /// of the section to which the relocation applies.
        /// 
        /// If the section contains symbol information, this field holds the number of local symbols -
        /// also the index of the first non-local symbol.
        /// </summary>
        public uint Info { get; private set; }
        /// <summary>
        /// The required alignment of the section. Must be 0, 1 or a power of 2.
        /// </summary>
        public long Alignment { get; private set; }
        /// <summary>
        /// For sections that contain fixed-size entries, contains the size (bytes) of each entry.
        /// </summary>
        public long EntrySize { get; private set; }


        /// <summary>
        /// Creates a new Section object from the given binary data stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="Is64Bit">Whether the executable has a 64-bit format.</param>
        /// <exception cref="System.BadImageFormatException"/>
        public Section(BinaryReader reader, bool Is64Bit)
        {
            this.NameIndex   = reader.ReadUInt32();
            this.Type        = (SectionType)reader.ReadUInt32();
            this.Flags       = (SectionFlags)(Is64Bit ? reader.ReadUInt64() : reader.ReadUInt32());
            this.LoadAddress = Is64Bit ? reader.ReadUInt64() : reader.ReadUInt32();
            this.FileOffset  = Is64Bit ? reader.ReadInt64()  : reader.ReadUInt32();
            this.Size        = Is64Bit ? reader.ReadInt64()  : reader.ReadUInt32();

            this.LinkedSectionIndex = reader.ReadInt32();
            this.Info               = reader.ReadUInt32();
            this.Alignment          = Is64Bit ? reader.ReadInt64() : reader.ReadUInt32();
            this.EntrySize          = Is64Bit ? reader.ReadInt64() : reader.ReadUInt32();

            if (this.FileOffset < 0 || this.Size < 0 || this.LinkedSectionIndex < 0 ||
                this.Alignment < 0  || this.EntrySize < 0)
            {
                throw new BadImageFormatException("Section exceeds maximum supported size " +
                                                  "or alignment");
            }
        }

        /// <summary>
        /// Sets the name of the section to the given string.
        /// Throws an exception if the name has already been set.
        /// </summary>
        /// <param name="name">The string to which to assign the section name</param>
        /// <exception cref="System.InvalidOperationException"/>
        public void SetName(string name)
        {
            if (this.Name != null) {
                throw new InvalidOperationException("Section name already loaded");
            }
            else this.Name = name;
        }

    }
}
