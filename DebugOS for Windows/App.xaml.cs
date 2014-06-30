/* App.xaml.cs - (c) James S Renwick 2013
 * --------------------------------------
 * Version 1.3.1
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

        /// <summary>
        /// Loads the debugger with the specified name. Exception neutral.
        /// </summary>
        /// <param name="name">The name of the debugger to load. Case sensitive.</param>
        internal static void LoadDebugger(string name)
        {
            foreach (Extension extension in LoadedExtensions)
            {
                if (extension.Name == name && extension.HasDebugger)
                {
                    try { // Load debugger
                        Application.Debugger = extension.LoadDebugger();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Error loading debugger '{0}': {1}", name, e);
                        throw; // Propagate
                    }
                    try
                    {
                        // Now load object file
                        if (Application.Session != null && System.IO.File.Exists(Application.Session.ImageFilepath))
                        {
                            ObjectCodeFile obj = Loaders.ObjectFileLoader.Load(Application.Session.ImageFilepath, AssemblySyntax.Intel);
                            Application.Debugger.IncludeObjectFile(obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Error loading debug image '{0}': {1}", Application.Session.ImageFilepath, e);
                        throw; // Propagate
                    }
                }
            }
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
                new Extension(new BreakpointExtension()),
                new Extension(new ExecutionExtension()),
                new Extension(new AssemblyExplorerExtension())
            };

            // Gets the application directory if available, otherwise defaults to the CWD.
            App.ApplicationDirectory = new DirectoryInfo(Path.GetDirectoryName(
                Environment.GetCommandLineArgs()[0]));

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

            // Call base method
            base.OnStartup(startup); 
        }
    }
}
