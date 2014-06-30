/* PEImage.cs - (c) James S Renwick 2014
 * -------------------------------------
 * Version 1.0.0
 * 
 */

using System;
using System.IO;

using DebugOS.COFF;

namespace DebugOS.PE
{
    /// <summary>
    /// Data structure holding the information defined by a Portable 
    /// Executable image header.
    /// </summary>
    public class PEImage
    {
        /// <summary>The offset within the file of the PE signature offset.</summary>
        private const int SIGNATURE_OFFSET_OFFSET = 0x3C;

        /// <summary>
        /// Gets the COFF header. Contains details on the layout of the image.
        /// </summary>
        public COFFHeader Header { get; private set; }
        /// <summary>
        /// Gets the sections defined for this image.
        /// </summary>
        public SectionHeader[] Sections { get; private set; }
        /// <summary>
        /// Gets an optional extended COFF header. Contains information
        /// useful in loading the binary. Can be null.
        /// </summary>
        public OptionalHeader HeaderEx { get; private set; }
        /// <summary>
        /// Gets an optional Windows-specific header. Can be null.
        /// </summary>
        public WindowsHeader WindowsHeader { get; private set; }
        /// <summary>
        /// Gets a catalogue of PE data-directories.
        /// </summary>
        public DataDirectories DataDirectories { get; private set; }
        /// <summary>
        /// Gets the COFF symbols (if any) defined in the symbol table.
        /// </summary>
        public Symbol[] Symbols { get; private set; }


        /// <summary>
        /// Loads a new PEImage object from the given data stream.
        /// </summary>
        /// <param name="reader">The reader to use. Must support seeking.</param>
        /// 
        /// <exception cref="System.BadImageFormatException">
        /// Thrown when the image format is invalid.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the given BinaryReader's base stream does not support seek operations.
        /// </exception>
        /// 
        public PEImage(BinaryReader reader)
        {
            // Must be able to seek
            if (!reader.BaseStream.CanSeek)
            {
                throw new InvalidOperationException(
                    "Invalid data stream - must have seek capability.");
            }

            // Read and validate the signature offset
            reader.BaseStream.Seek(SIGNATURE_OFFSET_OFFSET, SeekOrigin.Begin);
            long sigOffset = reader.ReadUInt32();

            if (sigOffset <= SIGNATURE_OFFSET_OFFSET || 
                sigOffset >= reader.BaseStream.Length)
            {
                throw new BadImageFormatException("Invalid PE image signature");
            }

            // Now read and validate the signature
            reader.BaseStream.Seek(sigOffset, SeekOrigin.Begin);
            byte[] sig = reader.ReadBytes(4);

            if (sig[0] != 'P' || sig[1] != 'E' || sig[2] != '\0' || sig[3] != '\0') {
                throw new BadImageFormatException("Invalid PE image signature");
            }

            // Now read the COFF Header
            this.Header   = new COFFHeader(reader);
            this.Sections = new SectionHeader[this.Header.SectionCount];

            int optHeaderSize = this.Header.OptionalHeaderSize;

            // Now read the optional header if one is present
            if (optHeaderSize != 0)
            {
                if (optHeaderSize < OptionalHeader.RAW_DATA_SIZE) {
                    throw new BadImageFormatException("Invalid COFF optional header size");
                }

                this.HeaderEx = new OptionalHeader(reader, optHeaderSize);

                // Load Windows header & data-directories as specified
                if (optHeaderSize > OptionalHeader.RAW_DATA_SIZE)
                {
                    this.WindowsHeader = new WindowsHeader(reader,
                        this.HeaderEx.Magic == OptionalHeader.MAGIC_PE32PLUS);

                    long ddirs = this.WindowsHeader.DataDirectoryCount;
                    this.DataDirectories = new DataDirectories(reader, ddirs);
                }
                else
                {
                    // Create an empty data-directories object
                    this.DataDirectories = new DataDirectories(reader, 0);
                }
            }
            
            // Read each section header
            for (int i = 0; i < this.Header.SectionCount; i++)
            {
                this.Sections[i] = new SectionHeader(reader);
            }

            // Read symbol info if available
            long symbolCount = this.Header.SymbolCount;

            this.Symbols = new Symbol[symbolCount];
            if (symbolCount != 0 && this.Header.SymbolTableOffset != 0)
            {
                reader.BaseStream.Seek(this.Header.SymbolTableOffset, SeekOrigin.Begin);

                for (long i = 0; i < symbolCount; i++) {
                    this.Symbols[i] = new Symbol(reader);
                }
            }
        }
        
    }
}
