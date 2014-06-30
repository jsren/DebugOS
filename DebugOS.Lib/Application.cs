using System;

namespace DebugOS
{
    public static class Application
    {
        private static IDebugger debugger;
        private static DebugSession session;


        public static event Action SessionChanged;
        public static event Action DebuggerChanged;


        public static DebugSession Session
        {
            get { return session; }

            set
            {
                if (debugger != null && debugger.CurrentStatus != DebugStatus.Disconnected) {
                    throw new InvalidOperationException("Cannot change session while debugging in progress");
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
                    throw new InvalidOperationException("Cannot change debugger while debugging in progress");
                }
                else
                {
                    if (session != null) {
                        session.Debugger = value.Name;
                    }
                    debugger = value;

                    if (DebuggerChanged != null) DebuggerChanged();
                }
            }
        }

    }
}
