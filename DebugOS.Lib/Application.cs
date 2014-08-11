/* Application.cs - (c) James S Renwick 2014
 * -----------------------------------------
 * Version 1.4.0
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DebugOS
{
    public static class Application
    {
        private static IDebugger    debugger;
        private static DebugSession session;

        public static event Action SessionChanged;
        public static event Action DebuggerChanged;
        
        /// <summary>
        /// Gets an ArgumentSet containing the parameters passed to
        /// the application.
        /// </summary>
        public static ArgumentSet Arguments { get; private set; }
        /// <summary>
        /// Gets the directory in which the application is installed.
        /// </summary>
        public static DirectoryInfo Directory { get; private set; }
        /// <summary>
        /// Gets an array of the supported architectures.
        /// </summary>
        public static Architecture[] LoadedArchitectures { get; private set; }
        /// <summary>
        /// Gets an array of the loaded extensions.
        /// </summary>
        public static Extension[] LoadedExtensions { get; private set; }


        /// <summary>
        /// Gets or sets the current debugging session.
        /// 
        /// The session can only be set when the current debugger is null 
        /// (Nothing in Visual Basic) or the debugger is disconnected.
        /// </summary>
        public static DebugSession Session
        {
            get { return session; }

            set
            {
                if (debugger != null && debugger.CurrentStatus != DebugStatus.Disconnected) {
                    throw new InvalidOperationException("Cannot change session while debugging in progress.");
                }
                else session = value;

                // Fire changed event
                if (SessionChanged != null) SessionChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current debugger.
        /// 
        /// The debugger can only be set when the current debugger is null 
        /// (Nothing in Visual Basic) or the debugger is disconnected.
        /// </summary>
        public static IDebugger Debugger
        {
            get { return debugger; }

            set
            {
                if (debugger != null && debugger.CurrentStatus != DebugStatus.Disconnected) {
                    throw new InvalidOperationException("Cannot change debugger while debugging in progress.");
                }
                else debugger = value;

                // Fire changed event
                if (DebuggerChanged != null) DebuggerChanged();
            }
        }

        
        /// <summary>
        /// Performs initial application resource loading.
        /// </summary>
        static Application()
        {
            // === Load Application Environment ===

            string[] args = Environment.GetCommandLineArgs();
            string[] tmp  = new string[args.Length - 1];

            Array.Copy(args, 1, tmp, 0, tmp.Length);
            Arguments = new ArgumentSet(tmp);

            // Gets the application directory if available, otherwise defaults to the CWD.
            Application.Directory = new DirectoryInfo(Path.GetDirectoryName(
                Environment.GetCommandLineArgs()[0]));

            // === Load Architectures ===

            var architectures = new List<Architecture>();

            // Gets the application's architectures directory
            string archdir = Path.Combine(Directory.FullName, "Architectures");

            // Scan the architecture directory (if it exists) for extensions to load
            if (System.IO.Directory.Exists(archdir))
            {
                foreach (string filename in System.IO.Directory.EnumerateFiles(archdir))
                {
                    if (Path.GetExtension(filename) == ".xml")
                    {
                        try
                        {
                            architectures.Add(Architecture.FromStream(File.OpenRead(filename)));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[ERROR] Error loading architecture: " + e.Message);
                        }
                    }
                }
            }
            else try { System.IO.Directory.CreateDirectory(archdir); } catch { }

            // Register the loaded architectures
            LoadedArchitectures = architectures.ToArray();
        }

        /// <summary>
        /// Loads a fixed set of extensions from those given and those
        /// loaded from the extension directory.
        /// 
        /// This can only be called once and from an internal assembly.
        /// </summary>
        /// <param name="additional">Additional extensions to add to the application's list.</param>
        public static void LoadExtensions(IEnumerable<Extension> additional = null)
        {
            // Assert that it's an internal call
            Utils.AssertInternal();

            // Don't allow re-loading
            if (Application.LoadedExtensions != null)
            {
                throw new InvalidOperationException();
            }
            List<Extension> extensions = (additional ?? new List<Extension>()).ToList();

            // Get the application's extension directory
            string extdir = Path.Combine(Application.Directory.FullName, "Extensions");

            // Scan the extension directory (if it exists) for extensions to load
            if (System.IO.Directory.Exists(extdir))
            {
                foreach (string filename in System.IO.Directory.EnumerateFiles(extdir))
                {
                    if (Path.GetExtension(filename) == ".dll")
                    {
                        try
                        {
                            extensions.AddRange(Loaders.ExtensionLoader.
                                LoadExtensionAssembly(filename));                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[ERROR] Error loading extension: " + e.Message);
                        }
                    }
                }
            }
            else try { System.IO.Directory.CreateDirectory(extdir); } catch { }

            // Now register extensions & initialise
            foreach (Extension ext in Application.LoadedExtensions = extensions.ToArray())
            {
                try {
                    ext.Initialise(Environment.GetCommandLineArgs());
                }
                catch (Exception e) {
                    Console.WriteLine("[WARNING] Error loading extension '{0}': {1}", ext.Name, e.Message);
                }
            }
        }
    }
}
