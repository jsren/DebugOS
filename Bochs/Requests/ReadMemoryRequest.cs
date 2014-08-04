using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DebugOS.Bochs.Requests
{
    internal class ReadMemoryRequest : IBochsRequest
    {
        static readonly Regex memoryRegex = 
            new Regex(BochsConnector.addr + @" <.*\+.*>:\s+(.*)");

        private int totalRead;
        private byte[] bytes;
        private Action<byte[]> callback;

        public bool isComplete
        {
            get { return this.totalRead == bytes.Length; }
        }
        
        public ReadMemoryRequest(int requestedBytes, Action<byte[]> callback)
        {
            this.totalRead = 0;
            this.callback  = callback;
            this.bytes     = new byte[requestedBytes];
        }

        public bool feedLine(string line)
        {
            Match match = memoryRegex.Match(line);
            if (!match.Success) return false;

            string[] addresses = match.Groups[1].Value.Split('\t', ' ');

            // Don't put this in a try until we've tested it
            // TODO: Try this
            foreach (String hex in addresses)
            {
                this.bytes[totalRead++] = Utils.ParseHex8(hex);
            }
            return true;
        }

        public void handleComplete()
        {
            // Invoke callback async
            if (this.callback != null)
            {
                callback.BeginInvoke(this.bytes, callback.EndInvoke, null);
            }
        }
    }
}
