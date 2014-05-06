/* App.xaml.cs - (c) James S Renwick 2013
 * --------------------------------------
 * Version 1.2.3
 * 
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
    public partial class App : Application
    {
        private static IDebugger dbg = null;


        /// <summary>
        /// Event fired when a debugger is registered and initialised.
        /// </summary>
        public static event Action DebuggerRegistered;

        /// <summary>
        /// Gets the path to the debug image.
        /// </summary>
        public static string ImagePath { get; internal set; }

        /// <summary>
        /// Gets the current splash screen.
        /// </summary>
        public static SplashWindow SplashScreen { get; private set; }

        /// <summary>
        /// Gets an array of the loaded extensions.
        /// </summary>
        public static Extension[] LoadedExtensions { get; private set; }

        /// <summary>
        /// Gets the current MainWindow.
        /// </summary>
        /// <returns>The current instance of MainWindow or null.</returns>
        public static MainWindow GetMainWindow() { return App.Current.MainWindow as MainWindow; }

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
        /// Gets or sets the current debugger.
        /// Once set to a non-null value, this field is frozen
        /// and will throw an exception upon calling the setter.
        /// </summary>
        public static IDebugger Debugger
        {
            get { return dbg; }

            set
            {
                if (dbg != null)
                {
                    throw new Exception("Debugger can only be set once.");
                }
                else
                {
                    dbg = value;

                    if (DebuggerRegistered != null) {
                        DebuggerRegistered();
                    }
                }
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
                if (extension.Name == name && extension.HasDebugger)
                {
                    try
                    {
                        // Load debugger
                        App.Debugger = extension.LoadDebugger();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Error loading debugger '{0}': {1}", name, e);
                        throw;
                    }
                    try
                    {
                        // Now load object file
                        if (System.IO.File.Exists(App.ImagePath))
                        {
                            var obj = DebugOS.Loaders.ObjectFileLoader.Load(App.ImagePath, AssemblySyntax.Intel);
                            App.Debugger.IncludeObjectFile(obj);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Error loading debug image '{0}': {1}", App.ImagePath, e);
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="e">The application's command-line arguments.</param>
        protected override void OnStartup(StartupEventArgs startup)
        {
            // Show dynamic splash screen
            App.SplashScreen = SplashWindow.ShowSplash();
            App.SplashScreen.ShowBGImage = false;

            var extensions = new List<Extension>()
            {
                new Extension(new ExecutionExtension()),
                new Extension(new BochsExtension()),
                new Extension(new BreakpointExtension())
            };

            // Scan the extension directory (if it exists) for extensions to load
            if (Directory.Exists("Extensions"))
            {
                foreach (string filename in Directory.EnumerateFiles("Extensions"))
                {
                    if (Path.GetExtension(filename) == ".dll")
                    {
                        try {
                            extensions.AddRange(ExtensionLoader.loadExtensionAssembly(Assembly.LoadFrom(filename)));                            
                        }
                        catch (Exception e) {
                            Console.WriteLine("[ERROR] Error loading extension: " + e.Message);
                        }
                    }
                }
            }
            // Register the loaded extensions
            App.LoadedExtensions = extensions.ToArray();


            /* == TODO - THEMES == */

            // Now load either the selected theme or just use default
            var defaultTheme = new Uri("pack://application:,,,/dbgos;component/Themes/Beach.xaml");
            this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = defaultTheme });

            //var customTheme  = new Uri("pack://siteoforigin:,,,/Tundra.xaml", UriKind.RelativeOrAbsolute);
            //this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = customTheme });

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

            base.OnStartup(startup); // Call base method
        }
    }
}
