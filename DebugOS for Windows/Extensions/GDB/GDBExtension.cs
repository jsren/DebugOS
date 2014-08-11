using System;

namespace DebugOS.Extensions
{
    public sealed class GDBExtension : IDebuggerExtension
    {
        string host = null;
        int    port = 0;

        public string Name
        {
            get { return "GDB Debugger Extension"; }
        }
        /// <summary>
        /// Gets the name of the debugger.
        /// </summary>
        string IDebuggerExtension.Name
        {
            get { return "GDB Debugger"; }
        }


        public void Initialise(string[] args)
        {
            Application.SessionChanged += () =>
            {
                this.OnUpdateSessionProps();
            };
        }

        private void OnUpdateSessionProps()
        {
            if (Application.Session != null)
            {
                // Try and get host
                this.host = this.host ?? Application.Session.Properties["GDBDebugger.Host"];

                // Try and get port
                if (this.port == 0)
                {
                    int.TryParse(Application.Session.Properties["GDBDebugger.Port"], out port);
                }
            }
        }

        public IDebugger LoadDebugger()
        {
            this.OnUpdateSessionProps();

            if (host == null || port == 0)
            {
                var dialog = new GDBConfiguration();
                var res    = dialog.ShowDialog();

                if (res.HasValue && res.Value)
                {
                    host = dialog.Host;
                    port = int.Parse(dialog.Port);
                }
                else throw new Exception("Debugger loading was cancelled");
            }

            Application.Session.Properties["GDBDebugger.Host"] = host;
            Application.Session.Properties["GDBDebugger.Port"] = port.ToString();
            return new GDB.GDBDebugger();
        }
    }
}
