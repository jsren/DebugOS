using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DebugOS.GDB
{
    public abstract class Request<T> : Request
    {
        public T Result { get; protected set; }

        public Action<Request> Callback { get; private set; }

        protected Request(Action<Request> callback) : base(true)
        {
            if (callback == null) throw new ArgumentNullException("callback");

            this.Callback = callback;
        }

        protected Request(Message msg) : base(msg, new AutoResetEvent(false), true) { }

        protected abstract T GetResult(Message message);

        internal override void HandleResponse(Message message)
        {
            this.Result = this.GetResult(message);

            // Signal on wait handle
            base.HandleResponse(message);

            if (this.Callback != null)
            {
                this.Callback(this);
            }
        }
    }

    public class Request : IDisposable
    {
        public bool HasResponseData { get; private set; }
        public EventWaitHandle Lock { get; private set; }
        public Message Response { get; private set; }
        public object Context { get; private set; }
        public bool? Acknowledgement { get; private set; }
        public Message Message { get; private set; }
        public bool Sent { get; internal set; }

        public Request(Message msg, EventWaitHandle waitLock, bool hasData)
        {
            if (waitLock == null) throw new ArgumentNullException("waitLock");

            this.Lock            = waitLock;
            this.HasResponseData = hasData;
            this.Message         = msg;
        }

        protected Request(bool hasData)
        {
            this.HasResponseData = hasData;
        }

        internal void Acknowledge(bool result)
        {
            this.Acknowledgement = result;
        }

        internal virtual void HandleResponse(Message message)
        {
            this.Response = message;

            if (this.Lock != null)
            {
                // The lock may have been disposed already, so the request thread no longer cares.
                // In that case, just swallow the exception.
                try { this.Lock.Set(); }
                catch (ObjectDisposedException) { }
            }
        }
    
        public void Dispose()
        {
 	        if (this.Lock != null)
            {
                this.Lock.Dispose();
            }
        }
    }
}
