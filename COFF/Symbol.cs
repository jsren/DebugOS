/* Symbol.cs - Logic (c) James S Renwick 2014
 * ------------------------------------------
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
    /// An entry in a COFF symbol table. Indentifies a symbol within an
    /// executable image or object file.
    /// </summary>
    [Serializable]
    public sealed class Symbol
    {
        /// <summary>The size (bytes) of the COFF symbol entry.</summary>
        internal const int RAW_DATA_SIZE = 8;

        /// <summary>
        /// The name of the symbol.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The value that is associated with the symbol. 
        /// 
        /// The interpretation of this field depends on SectionNumber and 
        /// StorageClass. A typical meaning is the relocatable address.
        /// </summary>
        public long Value { get; private set; }
        /// <summary>
        /// The signed integer that identifies the section using a one-based index 
        /// into the section table. 
        /// 
        /// A value of 0 indicates the symbol has yet to be assigned a section. 
        /// 
        /// A value of -1 indicates the symbol has an absolute address and its 
        /// value is not an address. 
        /// 
        /// A value of -2 indicates the symbol provides metadata but has no section.
        /// </summary>
        public short Section { get; private set; }
        /// <summary>
        /// The base (data type) of the symbol.
        /// </summary>
        public SymbolBaseType BaseType { get; private set; }
        /// <summary>
        /// The complex type (meta type) of the symbol.
        /// </summary>
        public SymbolComplexType ComplexType { get; private set; }
        /// <summary>
        /// The storage class of the symbol.
        /// </summary>
        public StorageClass StorageClass { get; private set; }
        /// <summary>
        /// The number of auxiliary symbol table entries that follow this record.
        /// </summary>
        public int AuxSymbolCount { get; private set; }


        /// <summary>
        /// Reads the next symbol entry from the specified 
        /// binary data source.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public Symbol(BinaryReader reader)
        {
            this.Name           = new String(reader.ReadChars(8)).TrimEnd('\0');
            this.Value          = reader.ReadUInt32();
            this.Section        = reader.ReadInt16();
            this.BaseType       = (SymbolBaseType)reader.ReadByte();
            this.ComplexType    = (SymbolComplexType)reader.ReadByte();
            this.StorageClass   = (StorageClass)reader.ReadByte();
            this.AuxSymbolCount = reader.ReadByte();
        }
    }
}
