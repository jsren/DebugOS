/* DebugSession.cs - (c) James S Renwick 2014
 * ------------------------------------------
 * Version 1.5.4
 */
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;

using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace DebugOS
{
    [Serializable]
    public class DebugSession : IDeserializationCallback
    {
        /* Non-Serialised Fields */
        [NonSerialized]
        private string sessionFile;
        [NonSerialized]
        private SessionMetrics metrics;

        /* Serialised Fields */
        private string   debuggerName;
        private string   imageFilepath;
        private string[] extensionNames;

        private Architecture architecture;
        private SessionProperties properties;

        /* === Properties for Public Access === */

        /// <summary>
        /// Gets an object containing session runtime details.
        /// </summary>
        public SessionMetrics Metrics { get { return metrics; } }
        /// <summary>
        /// Gets an array containing the names of the currently-loaded extensions.
        /// </summary>
        public string[] LoadedExtensions { get { return extensionNames; } }
        /// <summary>
        /// Gets the filepath to the primary debug image.
        /// </summary>
        public string ImageFilepath { get { return imageFilepath; } }
        /// <summary>
        /// Gets the session's property values.
        /// </summary>
        public SessionProperties Properties { get { return properties; } }
        /// <summary>
        /// Gets the path to the file in which the session has been saved, or
        /// null (Nothing in Visual Basic) if the session is unsaved.
        /// </summary>
        public string BackingFile { get { return sessionFile; } }
        /// <summary>
        /// Gets the machine architecture under which debugging is performed.
        /// </summary>
        public Architecture Architecture { get { return architecture; } }

        /// <summary>
        /// Constructor for serialization.
        /// </summary>
        protected DebugSession()
        {
            this.metrics = new SessionMetrics(this);
        }
        public void OnDeserialization(object sender)
        {
            this.metrics = new SessionMetrics(this);
        }

        /// <summary>
        /// Creates a new DebugSession object.
        /// </summary>
        /// <param name="debuggerName">The name of the session debugger.</param>
        /// <param name="loadedExtensions">The names of the loaded extensions.</param>
        /// <param name="imageFilepath">The path to the primary executable image.</param>
        /// <param name="architecture">The executing machine architecture.</param>
        public DebugSession(string debuggerName, string[] loadedExtensions, 
            string imageFilepath, Architecture architecture) : this()
        {
            this.debuggerName   = debuggerName;
            this.extensionNames = loadedExtensions;
            this.imageFilepath  = imageFilepath;

            this.architecture = architecture;
            this.properties   = new SessionProperties();
        }

        /// <summary>
        /// Gets the name of the current debugger.
        /// </summary>
        public string Debugger
        {
            get { return debuggerName; }
        }

        /// <summary>
        /// Saves the DebugSession to the given file.
        /// </summary>
        /// <param name="filepath">The path to the file in which to save the DebugSession.</param>
        public void Save(string filepath)
        {
            using (var stream = System.IO.File.OpenWrite(filepath))
            {
                stream.SetLength(0);

                var serializer = new BinaryFormatter();
                serializer.Serialize(stream, this);

                this.sessionFile = filepath;
            }
        }

        /// <summary>
        /// Loads a DebugSession from the given file.
        /// </summary>
        /// <param name="filepath">The path to the file from which to load the DebugSession.</param>
        /// <returns>The DebugSession object loaded from the given file.</returns>
        public static DebugSession Load(string filepath)
        {
            using (var stream = System.IO.File.OpenRead(filepath))
            {
                var serializer = new BinaryFormatter();
                var output     = serializer.Deserialize(stream) as DebugSession;

                output.sessionFile = filepath;
                return output;
            }
        }
    }
}
