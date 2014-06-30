using System;
using System.Collections.Generic;
using System.Linq;

namespace DebugOS
{
    [Serializable]
    public class DebugSession
    {
        /* Serialised Fields */
        private string debuggerName;
        private string imageFilepath;
        private string[] extensionNames;
        private string[] symbolDirectories;
        private Dictionary<string, object> properties;

        /* Properties for Public Access */
        public string[] LoadedExtensions { get { return extensionNames; } }

        public string ImageFilepath { get { return imageFilepath; } }
        public string[] SymbolDirectories { get { return symbolDirectories; } }
        public Dictionary<string, object> Properties { get { return properties; } }

        public string Debugger
        { 
            get { return debuggerName; } 
            set { debuggerName = value; }
        }

        public DebugSession(string debuggerName, string[] loadedExtensions, 
            string imageFilepath, string[] symbolDirs)
        {
            this.debuggerName      = debuggerName;
            this.extensionNames    = loadedExtensions;
            this.imageFilepath     = imageFilepath;
            this.symbolDirectories = symbolDirs;

            this.properties = new Dictionary<string, object>();
        }
    }
}
