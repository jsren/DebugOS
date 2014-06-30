/* Characteristics.cs - Logic (c) James S Renwick 2014
 * ---------------------------------------------------
 * Version 1.0.0
 * 
 * NOTE: Much of the documentation herein is taken from the document
 *       "Microsoft Portable Executable and Common Object File Format Specification" 
 *       and remains the intellectual property of Microsoft.
 */
using System;

namespace DebugOS.COFF
{
    /// <summary>
    /// An enumeration of COFF binary attributes.
    /// </summary>
    [Flags]
    public enum Characteristics : ushort
    {
        None              = 0x0,
        /// <summary>
        /// Indicates that the file does not contain base relocations and must 
        /// therefore be loaded at its preferred base address. If the base address 
        /// is not available, the loader reports an error. 
        /// 
        /// Image only, Windows CE, and Windows NT and later. 
        /// </summary>
        NoBaseRelocations = 0x1,
        /// <summary>
        /// Image only. This indicates that the image file is valid and can be run. If this flag is not set, 
        /// it indicates a linker error.
        /// </summary>
        ExecutableImage   = 0x2,
        /// <summary>
        /// COFF line numbers have been removed. This flag is deprecated and should be zero.
        /// </summary>
        [Obsolete]
        NoLineNumbers     = 0x4,
        /// <summary>
        /// COFF symbol table entries for local symbols have been removed. This flag is deprecated and should 
        /// be zero.
        /// </summary>
        [Obsolete]
        NoLocalSymbols    = 0x8,
        /// <summary>
        /// Obsolete. Aggressively trim working set. This flag is deprecated for Windows 2000 and later and 
        /// must be zero.
        /// </summary>
        [Obsolete]
        AgressiveWsTrim   = 0x10,
        /// <summary>
        /// Application can handle > 2 GB addresses.
        /// </summary>
        LargeAddressAware = 0x20,
        /// <summary>
        /// Little endian: the least significant bit (LSB) precedes the most significant bit (MSB) in memory. 
        /// This flag is deprecated and should be zero.
        /// </summary>
        [Obsolete]
        LittleEndian      = 0x80,
        /// <summary>
        /// Machine is based on a 32-bit-word architecture.
        /// </summary>
        Machine32Bit      = 0x100,
        /// <summary>
        /// Debugging information is removed from the image file.
        /// </summary>
        NoDebugInfo       = 0x200,
        /// <summary>
        /// If the image is on removable media, fully load it and copy it to the swap file.
        /// </summary>
        RemovableRFS      = 0x400,
        /// <summary>
        /// If the image is on network media, fully load it and copy it to the swap file.
        /// </summary>
        NetworkRFS        = 0x800,
        /// <summary>
        /// The image file is a system file, not a user program.
        /// </summary>
        SystemFile        = 0x1000,
        /// <summary>
        /// The image file is a dynamic-link library (DLL). Such files are considered executable files for 
        /// almost all purposes, although they cannot be directly run.
        /// </summary>
        DLL               = 0x2000,
        /// <summary>
        /// The file should be run only on a uniprocessor machine.
        /// </summary>
        Uniprocessor      = 0x4000,
        /// <summary>
        /// Big endian: the MSB precedes the LSB in memory. This flag is deprecated and should be zero.
        /// </summary>
        [Obsolete]
        BigEndian         = 0x8000,
    }
}
