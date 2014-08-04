/* App.xaml.cs - (c) James S Renwick 2013
 * --------------------------------------
 * Version 1.5.2
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

using DebugOS.Extensions;

namespace DebugOS
{
    /// <summary>
    /// A DebugOS application.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Gets the current MainWindow.
        /// </summary>
        /// <returns>The current instance of MainWindow or null.</returns>
        public static new MainWindow MainWindow { get; internal set; }

        /// <summary>
        /// Gets the current splash screen.
        /// </summary>
        public static SplashWindow SplashScreen { get; private set; }

        /// <summary>
        /// Gets an array of the loaded extensions.
        /// </summary>
        public static Extension[] LoadedExtensions { get; private set; }

        /// <summary>
        /// Gets an array of the names of the loaded themes.
        /// </summary>
        public static string[] LoadedThemes { get; private set; }

        /// <summary>
        /// Gets an array of the supported architectures.
        /// </summary>
        public static Architecture[] LoadedArchitectures { get; private set; }

        /// <summary>
        /// Gets the directory in which the application is installed.
        /// </summary>
        public static DirectoryInfo ApplicationDirectory { get; private set; }

        /// <summary>
        /// Aborts the application.
        /// </summary>
        /// <param name="msg">A message to print on stdout.</param>
        private void Abort(string msg)
        {
            Console.WriteLine(msg);
            this.Shutdown(1);
        }


        internal static void SetTheme(string name)
        {
            /* Ignore the fact that it may not be in the loaded themes array.
             * The array only really applies once we begin caching.
             * We should still try and load the file, however. */
            try
            {
                // Remove the current theme (if applied).
                if (App.Current.Resources.MergedDictionaries.Count > 1) {
                    App.Current.Resources.MergedDictionaries.RemoveAt(1);
                }
                // TODO: Cache themes. For now, just re-load and apply.
                Uri location = new Uri(Path.Combine(ApplicationDirectory.FullName, "Themes\\"+name+".xaml"));
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = location });
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Error loading theme '{0}': {1}", name, e);
            }
        }


        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="e">The application's command-line arguments.</param>
        protected override void OnStartup(StartupEventArgs startup)
        {
            // Show dynamic splash screen
            App.SplashScreen = SplashWindow.ShowSplash(showImage:true);

            // Load internal extensions
            var extensions = new List<Extension>()
            {
                new Extension(new BochsRegLoader()),
                new Extension(new BochsExtension()),
                new Extension(new GDBExtension()),
                new Extension(new QemuExtension()),
                new Extension(new BreakpointExtension()),
                new Extension(new ExecutionExtension()),
                new Extension(new AssemblyExplorerExtension()),
                new Extension(new SessionExtension()),
                new Extension(new SessionViewExtension())
            };

            // Gets the application directory if available, otherwise defaults to the CWD.
            App.ApplicationDirectory = new DirectoryInfo(Path.GetDirectoryName(
                Environment.GetCommandLineArgs()[0]));


            var architectures = new List<Architecture>();

            // Gets the application's architectures directory
            string archdir = Path.Combine(ApplicationDirectory.FullName, "Architectures");

            // Scan the extension directory (if it exists) for extensions to load
            if (Directory.Exists(archdir))
            {
                foreach (string filename in Directory.EnumerateFiles(archdir))
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
            else try { Directory.CreateDirectory(archdir); } catch { }

            // Register the loaded architectures
            LoadedArchitectures = architectures.ToArray();


            // Gets the application's extension directory
            string extdir = Path.Combine(ApplicationDirectory.FullName, "Extensions");

            // Scan the extension directory (if it exists) for extensions to load
            if (Directory.Exists(extdir))
            {
                foreach (string filename in Directory.EnumerateFiles(extdir))
                {
                    if (Path.GetExtension(filename) == ".dll")
                    {
                        try {
                            extensions.AddRange(Loaders.ExtensionLoader.loadExtensionAssembly(Assembly.LoadFrom(filename)));                            
                        }
                        catch (Exception e) {
                            Console.WriteLine("[ERROR] Error loading extension: " + e.Message);
                        }
                    }
                }
            }
            else try { Directory.CreateDirectory(extdir); } catch { }

            // Register the loaded extensions
            App.LoadedExtensions = extensions.ToArray();

            /* == TODO - LOAD CONFIG FILE == */

            // Load and apply the built-in theme
            var defaultTheme = new Uri("pack://application:,,,/dbgos;component/Themes/Beach.xaml");
            this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = defaultTheme });

            // Load available themes
            var    themes   = new List<string>() { "Beach" };
            string themedir = Path.Combine(ApplicationDirectory.FullName, "Themes");

            if (Directory.Exists(themedir))
            {
                foreach (string filename in Directory.EnumerateFiles(themedir))
                {
                    if (Path.GetExtension(filename) == ".xaml") {
                        themes.Add(Path.GetFileNameWithoutExtension(filename));
                    }
                }
            }
            else try { Directory.CreateDirectory(themedir); } catch { }

            App.LoadedThemes = themes.ToArray();

            // Initialise the loaded extensions
            foreach (var extension in LoadedExtensions)
            {
                try {
                    extension.Initialise(startup.Args);
                }
                catch (Exception e) {
                    Console.WriteLine("[WARNING] Error loading extension '{0}': {1}", extension.Name, e.Message);
                }
            }

            // Handle session change - automatically attempt to load required debugger
            Application.SessionChanged += App.OnSessionChanged;

            // Call base method
            base.OnStartup(startup); 
        }


        /// <summary>
        /// Handle session change - load debugger
        /// </summary>
        private static void OnSessionChanged()
        {
            if (Application.Session == null) return;

            try
            {
                App.LoadDebugger(Application.Session.Debugger);
            }
            catch (Exception x)
            {
                try { Application.Debugger.Disconnect(); } catch { }

                var msg = String.Format("An error occurred while loading debugger '{0}':\n{1}",
                    Application.Session.Debugger, x);

                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Debugger = null;
            }
        }

        /// <summary>
        /// Loads the debugger with the specified name. Exception neutral.
        /// </summary>
        /// <param name="name">The name of the debugger to load. Case sensitive.</param>
        internal static void LoadDebugger(string name)
        {
            foreach (Extension extension in LoadedExtensions)
            {
                if (extension.HasDebugger)
                {
                    try { 
                        if (extension.DebuggerName != name) continue; 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Error accessing debugger name property '{0}': {1}", extension.Name, e);
                        continue;
                    }

                    IDebugger newDebugger;

                    try { // Load debugger
                        newDebugger = extension.LoadDebugger();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Error loading debugger '{0}': {1}", name, e);
                        throw; // Propagate
                    }

                    // Now load object file(s)
                    if (Application.Session != null)
                    {
                        var assemblies = new List<Tuple<string, long>>();

                        if (!Application.Session.Properties.ContainsKey("DebugOS.LoadedAssemblies"))
                        {
                            assemblies.Add(Tuple.Create(Application.Session.ImageFilepath, -1L));
                        }
                        else
                        {
                            foreach (string entry in
                                Application.Session.Properties["DebugOS.LoadedAssemblies"].Split('?'))
                            {
                                long     loadAddr;
                                string[] details = entry.Split('*');

                                if (details.Length == 2 && Int64.TryParse(details[1], out loadAddr))
                                {
                                    assemblies.Add(Tuple.Create(details[0], loadAddr));
                                }
                            }
                        }

                        // Add the required object files
                        try
                        {
                            foreach (Tuple<string, long> assembly in assemblies)
                            {
                                ObjectCodeFile obj = Loaders.ObjectFileLoader.Load(assembly.Item1, AssemblySyntax.Intel);

                                if (assembly.Item2 != -1)
                                {
                                    obj.ActualLoadAddress = assembly.Item2;
                                }
                                newDebugger.IncludeObjectFile(obj);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[ERROR] Error loading debug image '{0}': {1}",
                                Application.Session.ImageFilepath, e);
                        }

                        // Assign debugger and propogate change
                        Application.Debugger = newDebugger;
                    }
                }
            }
        }
    }
}
