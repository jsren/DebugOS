using System;
using System.Reflection;
using System.Threading;
using System.Windows;

using Dispatcher = System.Windows.Threading.Dispatcher;

namespace DebugOS
{
	/// <summary>
	/// A WPF dynamic splash screen.
    /// Use SplashWindow.ShowSplash() to open a new splash.
	/// </summary>
	public partial class SplashWindow : Window
	{
        /// <summary>Initialises a new splash screen.</summary>
		public SplashWindow() {
            this.InitializeComponent();
		}

        /// <summary>Sets the progress message.</summary>
        public string Message
        {
            set
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    this.progressText.Text = value;
                });
            }
        }

        /// <summary>Gets or sets whether to display a background image.</summary>
        public bool ShowBGImage
        {
            set
            { 
                Dispatcher.BeginInvoke((Action)delegate
                {
                    this.bgImage.Opacity = value ? 1 : 0;
                });
            }
        }

        // Display assembly title & version
		private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            try
            {
                this.versionText.Text = "Version " + asm.GetName().Version.ToString();
                this.productText.Text = getAssemblyTitle(asm);
            }
            catch { }
		}
	}

    partial class SplashWindow
    {
        ///<summary>Gets the title of the current assembly.</summary>
        private static string getAssemblyTitle(Assembly asm)
        {
            var att = asm.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]
                as AssemblyTitleAttribute;
            return att == null ? "" : att.Title;
        }

        /// <summary>
        /// Creates and displays a new splash screen.
        /// </summary>
        /// <returns>The splash screen displayed.</returns>
        public static SplashWindow ShowSplash(bool showImage = true)
        {
            SplashWindow   output = null;
            AutoResetEvent evt    = new AutoResetEvent(false);

            var UIThread = new Thread(() =>
            {
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
                {
                    output = new SplashWindow()
                    {
                        ShowBGImage = showImage
                    };
                    evt.Set();
                    output.Show();
                });
                Dispatcher.Run();
            });

            UIThread.SetApartmentState(ApartmentState.STA);
            UIThread.IsBackground = true;
            UIThread.Name = "Splash Thread";
            UIThread.Start();

            evt.WaitOne();
            return output;
        }
    }
}