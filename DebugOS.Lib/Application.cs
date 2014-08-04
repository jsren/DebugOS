using System;

namespace DebugOS
{
    public static class Application
    {
        private static IDebugger debugger;
        private static DebugSession session;

        public static event Action SessionChanged;
        public static event Action DebuggerChanged;
        
        public static ArgumentSet Arguments { get; private set; }


        static Application()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2)
            {
                Arguments = new ArgumentSet(new string[0]);
            }
            else
            {
                string[] tmp = new string[args.Length - 1];
                Array.Copy(args, 1, tmp, 0, tmp.Length);

                Arguments = new ArgumentSet(tmp);
            }
        }


        public static DebugSession Session
        {
            get { return session; }

            set
            {
                if (debugger != null && debugger.CurrentStatus != DebugStatus.Disconnected) {
                    throw new InvalidOperationException("Cannot change session while debugging in progress.");
                }
                else session = value;

                if (SessionChanged != null) SessionChanged();
            }
        }

        public static IDebugger Debugger
        {
            get { return debugger; }

            set
            {
                if (debugger != null && debugger.CurrentStatus != DebugStatus.Disconnected) {
                    throw new InvalidOperationException("Cannot change debugger while debugging in progress.");
                }
                else
                {
                    debugger = value;
                    if (DebuggerChanged != null) DebuggerChanged();
                }
            }
        }

    }
}
