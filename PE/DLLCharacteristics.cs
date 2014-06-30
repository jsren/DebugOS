using System;

namespace DebugOS.PE
{
    [Flags]
    public enum DLLCharacteristics
    {
        /// <summary>
        /// DLL can be relocated at load time.
        /// </summary>
        DynamicBase = 0x40,
        /// <summary>
        /// Code Integrity checks are enforced.
        /// </summary>
        ForceIntegrity = 0x80,
        /// <summary>
        /// Image is NX compatible.
        /// </summary>
        NXCompat = 0x100,
        /// <summary>
        /// Isolation aware, but do not isolate the image.
        /// </summary>
        NoIsolation = 0x200,
        /// <summary>
        /// Does not use structured exception (SE) handling. 
        /// No SE handler may be called in this image.
        /// </summary>
        NoSEH = 0x400,
        /// <summary>
        /// Do not bind the image.
        /// </summary>
        NoBind = 0x800,
        /// <summary>
        /// A WDM driver.
        /// </summary>
        WDMDriver = 0x2000,
        /// <summary>
        /// Terminal Server aware.
        /// </summary>
        TerminalServerAware = 0x8000
    }
}
