/* MachineType.cs - Logic (c) James S Renwick 2014
 * -----------------------------------------------
 * Version 1.0.1
 * 
 * NOTE: Much of the documentation herein is taken from the document
 *       "Microsoft Portable Executable and Common Object File Format Specification" 
 *       and remains the intellectual property of Microsoft.
 */

namespace DebugOS.PE
{
    /// <summary>
    /// The COFF Machine Type of the Binary.
    /// </summary>
    public enum MachineType : ushort
    {
        /// <summary>
        /// Applies to any machine type.
        /// </summary>
        Unknown = 0x0,
        /// <summary>
        /// Matsushita AM33.
        /// </summary>
        AM33 = 0x1D3,
        /// <summary>
        /// x64
        /// </summary>
        AMD64 = 0x8664,
        /// <summary>
        /// ARM little endian.
        /// </summary>
        ARM = 0x1C0,
        /// <summary>
        /// ARMv7 (or higher) Thumb mode only.
        /// </summary>
        ARMNT = 0x1C4,
        /// <summary>
        /// ARMv8 in 64-bit mode.
        /// </summary>
        ARM64 = 0xAA64,
        /// <summary>
        /// EFI byte code.
        /// </summary>
        EBC = 0xEBC,
        /// <summary>
        /// Intel 386 or later processors and compatible processors.
        /// </summary>
        i386 = 0x14c,
        /// <summary>
        /// Intel Itanium processor family.
        /// </summary>
        IA64 = 0x200,
        /// <summary>
        /// Mitsubishi M32R little endian.
        /// </summary>
        M32R = 0x9041,
        /// <summary>
        /// MIPS16
        /// </summary>
        MIPS16 = 0x266,
        /// <summary>
        /// MIPS with FPU.
        /// </summary>
        MIPSFPU = 0x366,
        /// <summary>
        /// MIPS16 with FPU.
        /// </summary>
        MIPSFPU16 = 0x466,
        /// <summary>
        /// Power PC little endian.
        /// </summary>
        POWERPC = 0x1F0,
        /// <summary>
        /// Power PC with floating point support.
        /// </summary>
        POWERPCFP = 0x1F1,
        /// <summary>
        /// MIPS little endian.
        /// </summary>
        R4000 = 0x166,
        /// <summary>
        /// Hitachi SH3.
        /// </summary>
        SH3 = 0x1A2,
        /// <summary>
        /// Hitachi SH3 DSP.
        /// </summary>
        SH3DSP = 0x1A3,
        /// <summary>
        /// Hitachi SH4.
        /// </summary>
        SH4 = 0x1A6,
        /// <summary>
        /// Hitachi SH5.
        /// </summary>
        SH5 = 0x1A8,
        /// <summary>
        /// ARM or Thumb interworking.
        /// </summary>
        Thumb = 0x1C2,
        /// <summary>
        /// MIPS little-endian WCE v2.
        /// </summary>
        WCEMIPSV2 = 0x169
    }
}
