/* DebugSession.cs - (c) James S Renwick 2014
 * ------------------------------------------
 * Version 1.6.0
 */
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;

using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace DebugOS
{
    /// <summary>
    /// Object describing the current debugging session.
    /// </summary>
    [Serializable]
    public class DebugSession : IDeserializationCallback, IFreezable
    {
        /* Non-Serialised Fields */
        [NonSerialized]
        private string sessionFile;
        [NonSerialized]
        private SessionMetrics metrics;
        [NonSerialized]
        private List<ObjectCodeFile> loadedImages;

        /* Serialised Fields */
        private string   debuggerName;
        private string[] extensionNames;

        private Architecture architecture;
        private SessionProperties properties;

        /// <summary>
        /// Event fired when a new image is loaded into this session.
        /// </summary>
        public event EventHandler<ImageEventArgs> ImageLoaded;
        /// <summary>
        /// Event fired when an existing image is removed from this session.
        /// </summary>
        public event EventHandler<ImageEventArgs> ImageRemoved;

        /* === Properties for Public Access === */

        /// <summary>
        /// Gets the machine architecture under which debugging is performed.
        /// </summary>
        public Architecture Architecture { get { return architecture; } }
        /// <summary>
        /// Gets the path to the file in which the session has been saved, or
        /// null (Nothing in Visual Basic) if the session is unsaved.
        /// </summary>
        public string BackingFile { get { return sessionFile; } }
        /// <summary>
        /// Gets the name of the current debugger.
        /// </summary>
        public string Debugger { get { return debuggerName; } }
        /// <summary>
        /// Gets whether the object is in a "frozen" state.
        /// </summary>
        public bool IsFrozen { get; private set; }
        /// <summary>
        /// Gets an array containing the names of the currently-loaded extensions.
        /// </summary>
        public string[] LoadedExtensions { get { return extensionNames; } }
        /// <summary>
        /// Gets the ObjectCodeFiles which have been loaded for debugging.
        /// </summary>
        public ObjectCodeFile[] LoadedImages { get { return this.loadedImages.ToArray(); } }
        /// <summary>
        /// Gets an object containing session runtime details.
        /// </summary>
        public SessionMetrics Metrics { get { return metrics; } }
        /// <summary>
        /// Gets the session's property values.
        /// </summary>
        public SessionProperties Properties { get { return properties; } }
        
        

        /// <summary>
        /// Creates a new DebugSession object.
        /// </summary>
        protected DebugSession()
        {
            this.OnDeserialization(null);
        }
        /// <summary>
        /// Method called on object deserialization.
        /// </summary>
        public void OnDeserialization(object sender)
        {
            this.metrics      = new SessionMetrics(this);
            this.loadedImages = new List<ObjectCodeFile>();
        }

        /// <summary>
        /// Creates a new DebugSession object.
        /// </summary>
        /// <param name="debuggerName">The name of the session debugger.</param>
        /// <param name="loadedExtensions">The names of the loaded extensions.</param>
        /// <param name="architecture">The executing machine architecture.</param>
        public DebugSession(string debuggerName, string[] loadedExtensions, Architecture architecture) 
            : this(debuggerName, loadedExtensions, architecture, new SessionProperties())
        {

        }
        /// <summary>
        /// Creates a new DebugSession object.
        /// </summary>
        /// <param name="debuggerName">The name of the session debugger.</param>
        /// <param name="loadedExtensions">The names of the loaded extensions.</param>
        /// <param name="architecture">The executing machine architecture.</param>
        /// <param name="initialProperties">Initial session properties to set.</param>
        public DebugSession(string debuggerName, string[] loadedExtensions, Architecture architecture,
            SessionProperties initialProperties) : this()
        {
            if (initialProperties == null)
                throw new ArgumentNullException("initialProperties");
            if (architecture == null)
                throw new ArgumentNullException("architecture");

            this.debuggerName   = debuggerName;
            this.extensionNames = loadedExtensions ?? new string[0];
            this.architecture   = architecture;
            this.properties     = new SessionProperties(initialProperties);
        }

        /// <summary>
        /// Freezes the object, making members read-only.
        /// </summary>
        public void Freeze()
        {
            this.IsFrozen = true;
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
        /// Sets the name of the current debugger.
        /// </summary>
        /// <param name="name">The name of the current debugger.</param>
        public void SetDebuggerName(string name)
        {
            if (this.IsFrozen) throw new ObjectFrozenException();

            this.debuggerName = name;
        }

        /// <summary>
        /// Includes the given ObjectCodeFile in the list of
        /// current session images.
        /// </summary>
        /// <param name="file">The image to include.</param>
        public void IncludeImage(ObjectCodeFile image)
        {
            if (this.IsFrozen) throw new ObjectFrozenException();

            this.loadedImages.Add(image);

            if (this.ImageLoaded != null)
            {
                this.ImageLoaded(this, new ImageEventArgs(image));
            }
        }

        /// <summary>
        /// Removes the given ObjectCodeFile from the list of
        /// current session images.
        /// </summary>
        /// <param name="file">The image to remove.</param>
        /// <returns>True if removed, false if the item was not present.</returns>
        public bool RemoveImage(ObjectCodeFile image)
        {
            if (this.IsFrozen) throw new ObjectFrozenException();

            bool res = this.loadedImages.Remove(image);

            if (this.ImageRemoved != null)
            {
                this.ImageRemoved(this, new ImageEventArgs(image));
            }
            return res;
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
