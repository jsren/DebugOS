/* App.xaml.cs - (c) James S Renwick 2013
 * --------------------------------------
 * Version 1.6.0
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
        private static int popupCount = 0;

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
        /// Gets an array of the names of the loaded themes.
        /// </summary>
        public static string[] LoadedThemes { get; private set; }

        /// <summary>
        /// Gets whether a popup window is currently open.
        /// </summary>
        internal static bool IsPopupOpen { get { return popupCount != 0; } }

        /// <summary>
        /// Aborts the application.
        /// </summary>
        /// <param name="msg">A message to print on stdout.</param>
        private void Abort(string msg)
        {
            Console.WriteLine(msg);
            this.Shutdown(1);
        }

        internal static void OnPopupVisChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) popupCount++;
            else                  popupCount--;
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
                Uri location = new Uri(Path.Combine(Application.Directory.FullName, "Themes\\"+name+".xaml"));
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
            // Subscribe to unhandled exception events
            AppDomain.CurrentDomain.UnhandledException += OnAppUnhandledException;
            App.Current.DispatcherUnhandledException   += OnAppUnhandledException;

            // Show dynamic splash screen
            App.SplashScreen = SplashWindow.ShowSplash(showImage:true);


            // === TODO - LOAD CONFIG FILE === 

            // === Load Themes ===

            // Load and apply the built-in theme
            var defaultTheme = new Uri("pack://application:,,,/dbgos;component/Themes/Beach.xaml");
            this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = defaultTheme });

            // Load available themes
            var themes = new List<string>() { "Beach" };
            string themedir = Path.Combine(Application.Directory.FullName, "Themes");

            if (Directory.Exists(themedir))
            {
                foreach (string filename in Directory.EnumerateFiles(themedir))
                {
                    if (Path.GetExtension(filename) == ".xaml")
                    {
                        themes.Add(Path.GetFileNameWithoutExtension(filename));
                    }
                }
            }
            else try { Directory.CreateDirectory(themedir); } catch { }

            App.LoadedThemes = themes.ToArray();


            // === Load Extensions ===

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
                new Extension(new SymbolExplorerExtension()),
                new Extension(new SessionExtension()),
                new Extension(new SessionViewExtension()),

                new Extension(new Loaders.Objdump.ObjectFileLoader())
            };
            Application.LoadExtensions(extensions);

            
            // Handle session change - automatically attempt to load required debugger
            Application.SessionChanged += App.OnSessionChanged;

            // Call base method
            base.OnStartup(startup); 
        }

        /// <summary>
        /// Handle session change - load debugger.
        /// </summary>
        /// <remarks>Debugger should be disconnected before call.</remarks>
        private static void OnSessionChanged()
        {
            if (Application.Session == null) return;

            try
            {
                if (!Loader.LoadDebugger(Application.Session.Debugger))
                {
                    throw new Exception("Debugger not found");
                }
            }
            catch (Exception x)
            {
                var msg = String.Format("An error occurred while loading debugger '{0}':\n{1}",
                    Application.Session.Debugger, x);

                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Debugger = null;
            }
        }


        private void OnAppUnhandledException(object sender, System.Windows.Threading.
            DispatcherUnhandledExceptionEventArgs e)
        {
            this.OnAppUnhandledException(sender, 
                new UnhandledExceptionEventArgs(e.Exception, true));
        }
        private void OnAppUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Attempt to forcibly disconnect on termination
            if (e.IsTerminating)
            {
                try { Application.Debugger.Disconnect(); }
                catch { }
            }
        }
    }
}
