using System;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for StatusBar.xaml
	/// </summary>
	public partial class StatusBar : UserControl
	{
		public StatusBar()
		{
			this.InitializeComponent();

            // Check that a debugger has been loaded
            if (App.Debugger == null) return;

            this.UpdateStatus("Debugger Connected");

            App.Debugger.BreakpointHit += Debugger_BreakpointHit;
            App.Debugger.Stepped       += Debugger_Stepped;
            App.Debugger.Continued     += Debugger_Continued;
            App.Debugger.Disconnected  += Debugger_Terminated;
		}

        /// <summary>
        /// Sets the status text. This can be called safely from another thread.
        /// </summary>
        /// <param name="status">The string to set as the status.</param>
        public void UpdateStatus(string status)
        {
            Dispatcher.Invoke((Action)delegate
            {
                this.statusText.Text = status;
            });
        }
        
        /* Update Status Text */
        void Debugger_Continued(object sender, EventArgs e) {
            this.UpdateStatus("Waiting for Breakpoint");
        }
        void Debugger_Stepped(object sender, SteppedEventArgs e) {
            this.UpdateStatus("Debugging");
        }
        void Debugger_BreakpointHit(object sender, BreakpointHitEventArgs e) {
            this.UpdateStatus("Debugging");
        }
        void Debugger_Terminated(object sender, EventArgs e) {
            this.UpdateStatus("Terminated");
        }
	}
}