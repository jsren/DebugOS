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
        private UInt32[] dwords;
        private Action<UInt32[]> callback;

        public bool isComplete
        {
            get { return this.totalRead == dwords.Length; }
        }
        
        public ReadMemoryRequest(int requestedDWORDs, Action<UInt32[]> callback)
        {
            this.totalRead = 0;
            this.callback  = callback;
            this.dwords    = new UInt32[requestedDWORDs];
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
                dwords[totalRead++] = Utils.ParseHex32(hex);
            }
            return true;
        }

        public void handleComplete()
        {
            if (this.callback != null) {
                callback.BeginInvoke(this.dwords, callback.EndInvoke, null);
            }
        }
    }
}
