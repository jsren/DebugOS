/* SessionMetrics.cs - (c) James S Renwick 2014
 * --------------------------------------------
 * Version 1.1.0
 */
using System;

namespace DebugOS
{
    public class SessionMetrics
    {
        private DebugSession session;

        /// <summary>
        /// Gets the date and time at which the session began.
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// Gets the date and time at which the session ended.
        /// </summary>
        public DateTime EndTime { get; private set; }
        /// <summary>
        /// Gets the number of times a breakpoint has been hit since
        /// the session began.
        /// </summary>
        public long BreakpointHits { get; private set; }
        /// <summary>
        /// Gets the number of times the debugger has stepped since
        /// the session began.
        /// </summary>
        public long StepCount { get; private set; }

        /// <summary>
        /// Creates a new SessionMetrics object providing metrics
        /// for the given DebugSession.
        /// </summary>
        /// <param name="session">The DebugSession for to provide metrics.</param>
        public SessionMetrics(DebugSession session)
        {
            if (session == null) {
                throw new ArgumentNullException("session");
            }
            else this.session = session;

            Application.SessionChanged  += Application_SessionChanged;
            Application.DebuggerChanged += Application_DebuggerChanged;

            // Register handler for current debugger, if present
            this.Application_DebuggerChanged();
        }

        /// <summary>
        /// Called on debugger change.
        /// </summary>
        protected virtual void Application_DebuggerChanged()
        {
            if (Application.Debugger != null)
            {
                // Register handlers to track debug events
                Application.Debugger.Suspended += (s, e) =>
                {
                    this.StepCount++;

                    if (Application.Debugger.CurrentBreakpoint != null)
                    {
                        this.BreakpointHits++;
                    }
                };
            }
        }

        /// <summary>
        /// Called on session change.
        /// </summary>
        protected virtual void Application_SessionChanged()
        {
            if (Application.Session == this.session)
            {
                this.StartTime = DateTime.Now;
                this.Reset();
            }
            else if (this.StartTime != default(DateTime))
            {
                this.EndTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Resets the counters.
        /// </summary>
        private void Reset()
        {
            this.EndTime        = default(DateTime);
            this.BreakpointHits = 0;
            this.StepCount      = 0;
        }
    }
}
