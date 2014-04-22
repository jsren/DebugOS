using System;

namespace DebugOS
{
    /// <summary>
    /// Flags describing an object file section.
    /// </summary>
    [Flags]
    public enum SectionFlags
    {
        None,
        /// <summary>
        /// The section has data in the object file.
        /// </summary>
        Contents = 0x01,
        /// <summary>
        /// The section requires memory allocation.
        /// </summary>
        Alloc = 0x02,
        /// <summary>
        /// The section can be loaded into memory.
        /// </summary>
        Load = 0x04,
        /// <summary>
        /// ?
        /// </summary>
        Reloc = 0x08,
        /// <summary>
        /// The section is read-only.
        /// </summary>
        Readonly = 0x10,
        /// <summary>
        /// The section contains executable code.
        /// </summary>
        Code = 0x20,
        /// <summary>
        /// The section is not executable, but can be written to.
        /// </summary>*
        Data = 0x40,
        /// <summary>
        /// The section contains debug data.
        /// </summary>
        Debugging = 0x80
    }

    /// <summary>
    /// Object representing a section in an object file.
    /// </summary>
    public sealed class Section
    {
        /// <summary>The section's order within the object file.</summary>
        public int    Index { get; private set; }
        /// <summary>The section's name.</summary>
        public String Name  { get; private set; }
        /// <summary>The section's size in bytes.</summary>
        public long   Size  { get; private set; }

        /// <summary>
        /// The virtual memory address of the section. This is the address
        /// at which the section's contents will actually be loaded at runtime.
        /// </summary>
        public long VirtualMemoryAddress { get; private set; }
        /// <summary>
        /// The load memory address of the section. This is the address
        /// at which the section's contents will loaded intially from disk.
        /// </summary>
        public long LoadMemoryAddress { get; private set; }

        /// <summary>The section's offset in bytes within the file.</summary>
        public long FileOffset { get; private set; }
        /// <summary>The section's alignment in bytes.</summary>
        public int Alignment   { get; private set; }

        /// <summary>The section's flags.</summary>
        public SectionFlags Flags { get; private set; }

        /// <summary>Creates a blank section object.</summary>
        public Section() { }

        /// <summary>
        /// Creates a new section object.
        /// </summary>
        /// <param name="index">The section's order within the object file</param>
        /// <param name="name">The section's name.</param>
        /// <param name="size">The section's size in bytes.</param>
        /// <param name="runtimeAddress">The virtual memory address of the section. This is the address
        /// at which the section's contents will actually be loaded at runtime.</param>
        /// <param name="loadAddress">The load memory address of the section. This is the address
        /// at which the section's contents will loaded intially from disk.</param>
        /// <param name="fileOffset">The section's offset in bytes within the file.</param>
        /// <param name="alignment">The section's alignment in bytes.</param>
        /// <param name="flags">The section's flags.</param>
        public Section(int index, string name, long size, long runtimeAddress, 
            long loadAddress, long fileOffset, int alignment, SectionFlags flags)
        {
            this.Index      = index;
            this.Name       = name;
            this.Size       = size;
            this.FileOffset = fileOffset;
            this.Flags      = flags;
            this.VirtualMemoryAddress = runtimeAddress;
            this.LoadMemoryAddress    = loadAddress;
        }
    }
}
