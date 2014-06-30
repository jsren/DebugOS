/* OptionalHeader.cs - Logic (c) James S Renwick 2014
 * -------------------------------------------------
 * Version 1.0.0
 * 
 * NOTE: Much of the documentation herein is taken from the document
 *       "Microsoft Portable Executable and Common Object File Format Specification" 
 *       and remains the intellectual property of Microsoft.
 */

using System;
using System.IO;

namespace DebugOS.COFF
{
    /// <summary>
    /// Optional COFF header containing general image information 
    /// useful for loading and running an executable file.
    /// </summary>
    [Serializable]
    public sealed class OptionalHeader
    {
        /// <summary>The magic value for a ROM image.</summary>
        public const uint MAGIC_ROM = 0x107;
        /// <summary>The magic value for a PE32 or equivalent executable.</summary>
        public const uint MAGIC_EXECUTABLE = 0x10B;
        /// <summary>The magic value for a PE32+ executable.</summary>
        public const uint MAGIC_PE32PLUS = 0x20B;

        /// <summary>The size of the PE header data.</summary>
        public const int RAW_DATA_SIZE = 0x1C;
        

        /// <summary>
        /// Magic identifying the format of the image file.
        /// </summary>
        public uint Magic { get; private set; }
        /// <summary>
        /// The version of the linker used to produce the binary.
        /// </summary>
        public Version Version { get; private set; }
        /// <summary>
        /// The size of the code (text) section, or the sum of all code sections 
        /// if there are multiple sections.
        /// </summary>
        public long CodeSize { get; private set; }
        /// <summary>
        /// The size of the initialized data section, or the sum of all such 
        /// sections if there are multiple data sections.
        /// </summary>
        public long InitDataSize { get; private set; }
        /// <summary>
        /// The size of the uninitialized data section (BSS), or the sum of all 
        /// such sections if there are multiple BSS sections.
        /// </summary>
        public long UninitDataSize { get; private set; }
        /// <summary>
        /// The address of the entry point relative to the image base when the 
        /// executable file is loaded into memory. 
        /// 
        /// For program images, this is the starting address. 
        /// For device drivers, this is the address of the  initialization function. 
        /// An entry point is optional for DLLs. 
        /// 
        /// When no entry point is present, this field must be zero.
        /// </summary>
        public long EntryPointPtr { get; private set; }
        /// <summary>
        /// The address that is relative to the image base of the code section 
        /// when it is loaded into memory.
        /// </summary>
        public long CodeSectionPtr { get; private set; }
        /// <summary>
        /// The address that is relative to the image base of the data section 
        /// when it is loaded into memory.
        /// 
        /// This value is not read for PE32+ format images and will be set to 0.
        /// </summary>
        public long DataSectionPtr { get; private set; }


        /// <summary>
        /// Reads a COFF optional header from the given binary
        /// data stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="size">
        /// The size of the optional header as specified by the COFF file header.
        /// </param>
        public OptionalHeader(BinaryReader reader, int size)
        {
            if (size == 0 || size < RAW_DATA_SIZE) throw new ArgumentException();

            this.Magic          = reader.ReadUInt16();
            this.Version        = new Version(reader.ReadByte(), reader.ReadByte());
            this.CodeSize       = reader.ReadUInt32();
            this.InitDataSize   = reader.ReadUInt32();
            this.UninitDataSize = reader.ReadUInt32();
            this.EntryPointPtr  = reader.ReadUInt32();
            this.CodeSectionPtr = reader.ReadUInt32();

            // If the format is PE32+ and we have a windows header,
            // don't consume the final dword - just set to 0.
            if (this.Magic != MAGIC_PE32PLUS || size == RAW_DATA_SIZE)
            {
                this.DataSectionPtr = reader.ReadUInt32();
            }
            else this.DataSectionPtr = 0;
        }

    }
}
