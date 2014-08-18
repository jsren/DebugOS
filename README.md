#DebugOS

A fully-extensible graphical debugger written in C# boasting built-in integration with 
popular virtual machines/emulators such as Bochs and Qemu.

######Note: This product is still in ALPHA!


![DebugOS Screenshot](http://i.imgur.com/zuVtCgh.png)

####Features
- Supports binaries for most modern platforms
- Displays both annotated assembly and original source (where possible)
- Shows the current stack and platform-specific registers
- *Smart Context* enables instant lookup and breakpointing of addresses and symbols
- Integrates well with Bochs and Qemu, allowing you to quickly and effectively step through and breakpoint code
- *Smart Search* offers instant symbol, source file and breakpoint lookup for simple and fast navigation
- Complete and total extensibility: add custom/3rd-party views tailored to your OS; easily include support for other binaries and debuggers/VMs; integrate with the menubar, toolbar, *Smart Search* and *Smart Context* to enhance and customise your debugging workflow as you desire from any .NET object-oriented language.
- The core libraries are all written to be cross-platform, and plans are in the works for a WinForms/Mono UI implementation.

####Requirements
- Microsoft Windows with .NET Framework v4
- (For DIA support, a copy of Microsoft Visual Studio)
- For elf32-i386, elf32-little and elf32-big, a pre-built copy of objdump is included. For all other binaries, a valid objdump executable must either be on the system path or passed to DebugOS via the "*-objdump*" flag.
- Bochs, Qemu or a virtual machine/environment with a GDB stub

####TODO List/Caveats
- Some GDB (and thus Qemu) synchronisation issues and breakpoints being one-time (this is because GDB hangs on breakpoints.)
- Complete internal ELF & (PE)COFF libraries, doing away with objdump requirement for common binary formats
- Add extension-based source code colorizer support
- The binary load addresses are saved as part of a session, which may result in odd behaviour if your binary is subsequently re-based. For now, use the Assembly Explorer (*File->Configure Loaded Assemblies...*) to re-add the binary. This behaviour will change in a later release.

####The Legal Bit

All object code and markup (c) James S Renwick 2013-2014 unless otherwise stated.
Icons are copyright their respective owners and are free for non-commercial use.
Splash Image (c) Ryan Firth 2014.
Various program icons sourced from http://www.visualpharm.com.

"objdump" utility copyright 2013 Free Software Foundation, Inc. 
Licensed under the GNU General Public License version 3.

THIS PROGRAM COMES WITH ABSOLUTELY NO WARRANTY.

The author retains all rights. 
You may not alter or distribute the program or any derivatives without the permission of the author.
