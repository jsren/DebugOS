using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace DebugOS.Extensions
{
    public sealed class QemuExtension : IDebuggerExtension
    {
        private string   qemuPath    = null;
        private string   imagePath   = null;
        private string[] args        = null;
        private Process  qemuProcess = null;

        public string Name
        { 
            get { return "Qemu Extension"; }
        }
        string IDebuggerExtension.Name
        { 
            get { return "Qemu Debugger"; }
        }

        public IDebugger LoadDebugger()
        {
            // Some basic environment config
            Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "directx");
            Environment.SetEnvironmentVariable("QEMU_AUDIO_DRV", "dsound");
            Environment.SetEnvironmentVariable("SDL_AUDIODRIVER", "dsound");
            Environment.SetEnvironmentVariable("QEMU_AUDIO_LOG_TO_MONITOR", "1");

            if (this.imagePath == null)
            {
                using (var dialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    Filter          = "Image Files (*.img,*.iso)|*.img;*.iso|All Files (*.*)|*.*",
                    FilterIndex     = 0,
                    Multiselect     = false,
                    Title           = "Select CDROM image to debug"
                })
                {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.imagePath = dialog.FileName;
                }
                else throw new Exception("Unable to load Qemu - no image path specified.");
                    }
            }
            if (this.qemuPath == null)
            {
                using (var dialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    Filter          = "Qemu Executable (qemu-system-*.exe)|qemu-system-*.exe|All Files (*.*)|*.*",
                    FilterIndex     = 0,
                    Multiselect     = false,
                    Title           = "Select the Qemu executable"
                })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        this.qemuPath = dialog.FileName;
                    }
                    else throw new Exception("Unable to load Qemu - cannot locate executable.");
                }
            }

            // Set session properties
            Application.Session.Properties["QemuExtension.QemuPath"]  = this.qemuPath;
            Application.Session.Properties["QemuExtension.ImagePath"] = this.imagePath;

            // Load the debugger
            qemuProcess = Process.Start(new ProcessStartInfo(this.qemuPath)
            {
                Arguments = "-S -vga std -rtc base=localtime,clock=host -cdrom \""+
                            this.imagePath+"\" -gdb tcp:127.0.0.1:2200",

                WorkingDirectory = System.IO.Path.GetDirectoryName(this.qemuPath)
            });

            // Wait for idle
            qemuProcess.WaitForInputIdle(5000);

            // Check for exit
            if (qemuProcess.HasExited)
            {
                throw new Exception("Qemu has exited with code: " + qemuProcess.ExitCode);
            }

            // Now start the GDB session
            Application.Session.Properties["GDBDebugger.Host"] = "127.0.0.1";
            Application.Session.Properties["GDBDebugger.Port"] = "2200";

            foreach (Extension ext in Application.LoadedExtensions)
            {
                var dbg = ext.GetInterface<IDebuggerExtension>();

                if (dbg != null && dbg.Name == "GDB Debugger")
                {
                    return dbg.LoadDebugger();
                }
            }
            throw new Exception("Valid GDB debugger not found.");
        }

        public void Initialise(string[] args)
        {
            this.args = args; // Store for initializing GDB

            this.qemuPath  = Application.Arguments["-qemupath"];
            this.imagePath = Application.Arguments.Values.FirstOrDefault();

            Application.SessionChanged += () =>
            {
                if (Application.Session != null)
                {
                    this.qemuPath  = Application.Session.Properties["QemuExtension.QemuPath"] ?? this.qemuPath;
                    this.imagePath = Application.Session.Properties["QemuExtension.ImagePath"] ?? this.imagePath;
                }
            };
        }
    }
}
