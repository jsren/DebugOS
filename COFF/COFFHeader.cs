/* COFFHeader.cs - Logic (c) James S Renwick 2014
 * ----------------------------------------------
 * Version 1.0.0
 * 
 * NOTE: Much of the documentation herein is taken from the document
 *       "Microsoft Portable Executable and Common Object File Format Specification" 
 *       and remains the intellectual property of Microsoft.
 */

using System;
using System.IO;

using DebugOS.PE;

namespace DebugOS.COFF
{
    /// <summary>
    /// File header for a COFF executable image or object file.
    /// </summary>
    [Serializable]
    public sealed class COFFHeader
    {
        /// <summary>
        /// Gets the machine for which the binary was built.
        /// </summary>
        public MachineType MachineType { get; private set; }
        /// <summary>
        /// Gets the number of sections in the COFF binary.
        /// </summary>
        public int SectionCount { get; private set; }
        /// <summary>
        /// Gets the time at which the binary was built.
        /// </summary>
        public DateTime CreationDate { get; private set; }
        /// <summary>
        /// Gets the file offset of the COFF symbol table, if present.
        /// </summary>
        public long SymbolTableOffset { get; private set; }
        /// <summary>
        /// Gets the number of symbols in the COFF symbol table, if present.
        /// </summary>
        public long SymbolCount { get; private set; }
        /// <summary>
        /// Gets the size, in bytes, of the optional header.
        /// </summary>
        public int OptionalHeaderSize { get; private set; }
        /// <summary>
        /// Gets the COFF binary flags.
        /// </summary>
        public Characteristics Flags { get; private set; }

        /// <summary>
        /// Reads the COFF file header from the given binary data stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public COFFHeader(BinaryReader reader)
        {
            this.MachineType        = (MachineType)reader.ReadUInt16();
            this.SectionCount       = reader.ReadUInt16();
            this.CreationDate       = new DateTime(1970, 1, 1).AddSeconds(reader.ReadUInt32());
            this.SymbolTableOffset  = reader.ReadUInt32();
            this.SymbolCount        = reader.ReadUInt32();
            this.OptionalHeaderSize = reader.ReadUInt16();
            this.Flags              = (Characteristics)reader.ReadUInt16();
        }
    }
}
