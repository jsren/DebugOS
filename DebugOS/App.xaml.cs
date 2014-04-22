//#define UITEST

using DebugOS.Extensions;
using System;
using System.IO;
using System.Windows;

namespace DebugOS
{
    /// <summary>
    /// A DebugOS application.
    /// </summary>
    public partial class App : Application
    {
        private static IDebugger dbg = null;

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
                if (dbg != null) throw new Exception("Debugger can only be set once.");
                dbg = value;
            }
        }

        public static SplashWindow SplashScreen { get; private set; }

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
            Console.Write(msg);

            MessageBox.Show(msg, "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
            this.Shutdown(1);
        }

        /// <summary>
        /// The entry point for the application.
        /// </summary>
        /// <param name="e">The application's command-line arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Check for args
            if (e.Args.Length < 2) {
                Abort("Invalid command line paramaters");
            }

            // Show dynamic splash screen
            App.SplashScreen = SplashWindow.ShowSplash();
            App.SplashScreen.ShowBGImage = false;

            App.LoadedExtensions = new Extension[]
            {
                new Extension(new ExecutionExtension()),
                new Extension(new BochsExtension()),
                new Extension(new BreakpointExtension())
            };

            // Now load either the selected theme or just use default
            var defaultTheme = new Uri("pack://application:,,,/dbgos;component/Themes/Beach.xaml");
            this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = defaultTheme });

            //var customTheme  = new Uri("pack://siteoforigin:,,,/Tundra.xaml", UriKind.RelativeOrAbsolute);
            //this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = customTheme });

            foreach (var extension in LoadedExtensions)
            {
                try { extension.Initialise(e.Args); }
                catch (Exception x)
                {
                    Abort("[ERROR] Error loading extension: " + x.Message);
                    throw x;
                }
            }
#if UITEST
            // Just show UI already!
#else
            if (App.Debugger == null) {
                Abort("[ERROR] Error loading debugger."); return;
            }
            
#endif
            // Call base method
            base.OnStartup(e);
        }
    }
}
