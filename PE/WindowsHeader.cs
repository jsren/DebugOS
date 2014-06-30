using System;
using System.IO;

namespace DebugOS.PE
{
    /// <summary>
    /// A Windows-specific Portable Executable header.
    /// </summary>
    public sealed class WindowsHeader
    {
        /// <summary>
        /// The preferred address of the first byte of image when loaded into 
        /// memory. Must be a multiple of 64K.
        /// </summary>
        public ulong ImageBase { get; private set; }
        /// <summary>
        /// The alignment (in bytes) of sections when they are loaded into memory. 
        /// 
        /// It must be greater than or equal to FileAlignment. The default is the 
        /// page size for the architecture.
        /// </summary>
        public long SectionAlignment { get; private set; }
        /// <summary>
        /// The alignment factor (in bytes) that is used to align the raw data of 
        /// sections in the image file. 
        /// 
        /// The value should be a power of 2 between 512 and 64 K, inclusive. 
        /// The default is 512. If the SectionAlignment is less than the 
        /// architecture’s page size, then FileAlignment must match SectionAlignment.
        /// </summary>
        public long FileAlignment { get; private set; }
        /// <summary>
        /// The version of the required operating system.
        /// </summary>
        public Version TargetOSVersion { get; private set; }
        /// <summary>
        /// The version of the PE image.
        /// </summary>
        public Version ImageVersion { get; private set; }
        /// <summary>
        /// The version of the target subsystem.
        /// </summary>
        public Version SubsystemVersion { get; private set; }
        /// <summary>
        /// The size (in bytes) of the image, including all headers, as the image 
        /// is loaded in memory. It must be a multiple of SectionAlignment.
        /// </summary>
        public long ImageSize { get; private set; }
        /// <summary>
        /// The combined size of the MS DOS stub, PE header, and section headers 
        /// rounded up to a multiple of FileAlignment.
        /// </summary>
        public long HeaderSize { get; private set; }
        /// <summary>
        /// The image file checksum.
        /// </summary>
        public uint Checksum { get; private set; }
        /// <summary>
        /// The subsystem that is required to run this image.
        /// </summary>
        public Subsystem Subsystem { get; private set; }
        /// <summary>
        /// Characterists describing the current DLL.
        /// </summary>
        public DLLCharacteristics DLLFlags { get; private set; }
        /// <summary>
        /// The size of the stack to reserve. 
        /// 
        /// Only SizeOfStackCommit is committed; the rest is made available one 
        /// page at a time until the reserve size is reached.
        /// </summary>
        public ulong StackReserveSize { get; private set; }
        /// <summary>
        /// The size of the stack to commit.
        /// </summary>
        public ulong StackCommitSize { get; private set; }
        /// <summary>
        /// The size of the local heap space to reserve. 
        /// 
        /// Only SizeOfHeapCommit is committed; the rest is made available one 
        /// page at a time until the reserve size is reached.
        /// </summary>
        public ulong HeapReserveSize { get; private set; }
        /// <summary>
        /// The size of the local heap space to commit.
        /// </summary>
        public ulong HeapCommitSize { get; private set; }
        /// <summary>
        /// The number of data-directory entries in the remainder of the optional 
        /// header.
        /// </summary>
        public long DataDirectoryCount { get; private set; }


        /// <summary>
        /// Reads a windows-specific PE header from the given binary
        /// data stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="isPE32Plus">Whether the file format is PE32+.</param>
        public WindowsHeader(BinaryReader reader, bool isPE32Plus)
        {
            this.ImageBase = isPE32Plus ? reader.ReadUInt64() : reader.ReadUInt32();

            this.SectionAlignment = reader.ReadUInt32();
            this.FileAlignment    = reader.ReadUInt32();
            this.TargetOSVersion  = new Version(reader.ReadUInt16(), reader.ReadUInt16());
            this.ImageVersion     = new Version(reader.ReadUInt16(), reader.ReadUInt16());
            this.SubsystemVersion = new Version(reader.ReadUInt16(), reader.ReadUInt16());

            reader.ReadUInt32(); // Skip reserved field

            this.ImageSize  = reader.ReadUInt32();
            this.HeaderSize = reader.ReadUInt32();
            this.Checksum   = reader.ReadUInt32();
            this.Subsystem  = (Subsystem)reader.ReadUInt16();
            this.DLLFlags   = (DLLCharacteristics)reader.ReadUInt16();

            this.StackReserveSize = isPE32Plus ? reader.ReadUInt64() : reader.ReadUInt32();
            this.StackCommitSize  = isPE32Plus ? reader.ReadUInt64() : reader.ReadUInt32();
            this.HeapReserveSize  = isPE32Plus ? reader.ReadUInt64() : reader.ReadUInt32();
            this.HeapCommitSize   = isPE32Plus ? reader.ReadUInt64() : reader.ReadUInt32();

            reader.ReadUInt32(); // Skip reserved field

            this.DataDirectoryCount = reader.ReadUInt32();
        }
    }
}
