/* SectionHeader.cs - Logic (c) James S Renwick 2014
 * -------------------------------------------------
 * Version 1.1.0
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
    /// A COFF section table entry.
    /// Identifies and describes a single section.
    /// </summary>
    [Serializable]
    public sealed class SectionHeader
    {
        /// <summary>
        /// The name of the section.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// For native COFF:
        /// The address at which the section data should be loaded into memory. 
        /// For images, this is the absolute address within the program space. 
        /// For object files, this address is relative to the object's address space.
        /// 
        /// For PE binaries:
        /// The total size of the section when loaded into memory.
        /// 
        /// If this value is greater than SizeOfRawData, the section is zero-padded. 
        /// This field is valid only for executable images and should be set to zero 
        /// for object files.
        /// </summary>
        public long PhysicalAddressOrSize { get; private set; }
        /// <summary>
        /// For executable images, the address of the first byte of the section 
        /// relative to the image base when the section is loaded into memory. 
        /// 
        /// For object files, this field is the address of the first byte before 
        /// relocation is applied; for simplicity, compilers should set this to 
        /// zero. Otherwise, it is an arbitrary value that is subtracted from 
        /// offsets during relocation.
        /// </summary>
        public long VirtualAddress { get; private set; }
        /// <summary>
        /// The size of the section (for object files) or the size of the initialized
        /// data on disk (for image files). 
        /// 
        /// For executable images, this must be a 
        /// multiple of FileAlignment from the optional header. If this is less than 
        /// VirtualSize, the remainder of the section is zero-filled. 
        /// 
        /// Because the SizeOfRawData field is rounded but the VirtualSize field is 
        /// not, it is possible for SizeOfRawData to be greater than VirtualSize as 
        /// well. When a section contains only uninitialized data, this field should 
        /// be zero.
        /// </summary>
        public long RawDataLength { get; private set; }
        /// <summary>
        /// The file pointer to the first page of the section within the COFF file. 
        /// 
        /// For executable images, this must be a multiple of FileAlignment from the 
        /// optional header. For object files, the value should be aligned on a 
        /// 4-byte boundary for best performance. When a section contains only 
        /// uninitialized data, this field should be zero.
        /// </summary>
        public long RawDataOffset { get; private set; }
        /// <summary>
        /// The file pointer to the beginning of relocation entries for the section. 
        /// This is set to zero for executable images or if there are no relocations.
        /// </summary>
        public long RelocTableOffset { get; private set; }
        /// <summary>
        /// The file pointer to the beginning of line-number entries for the section. 
        /// This is set to zero if there are no COFF line numbers. This value should 
        /// be zero for an image because COFF debugging information is deprecated.
        /// </summary>
        public long LineNoTableOffset { get; private set; }
        /// <summary>
        /// The number of relocation entries for the section. This is set to zero 
        /// for executable images.
        /// </summary>
        public int RelocationCount { get; private set; }
        /// <summary>
        /// The number of line-number entries for the section. This value should be 
        /// zero for an image because COFF debugging information is deprecated.
        /// </summary>
        public int LineNoCount { get; private set; }
        /// <summary>
        /// The flags that describe the characteristics of the section.
        /// </summary>
        public PE.SectionFlags Flags { get; private set; }


        /// <summary>
        /// Reads a COFF section header from the given binary reader.
        /// </summary>
        /// <param name="reader">The binary data source.</param>
        public SectionHeader(BinaryReader reader)
        {
            this.Name                  = new String(reader.ReadChars(8)).TrimEnd('\0');
            this.PhysicalAddressOrSize = reader.ReadUInt32();
            this.VirtualAddress        = reader.ReadUInt32();
            this.RawDataLength         = reader.ReadUInt32();
            this.RawDataOffset         = reader.ReadUInt32();
            this.RelocTableOffset      = reader.ReadUInt32();
            this.LineNoTableOffset     = reader.ReadUInt32();
            this.RelocationCount       = reader.ReadUInt16();
            this.LineNoCount           = reader.ReadUInt16();
            this.Flags                 = (PE.SectionFlags)reader.ReadUInt32();
        }
    }
}
