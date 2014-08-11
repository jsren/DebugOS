/* Loader.cs - (c) James S Renwick 2014
 * ------------------------------------
 * Version 1.0.1
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DebugOS
{
    public static class Loader
    {
        public static void LoadStartup()
        {
            // This is an internal method, so assert internal
            Utils.AssertInternal();

            // First try to load from cmd-line args
            string architex = Application.Arguments["-a"] ?? Application.Arguments["-arch"];
            string debugger = Application.Arguments["-d"] ?? Application.Arguments["-debugger"];
            string session  = Application.Arguments["-s"] ?? Application.Arguments["-session"];

            // Load from a previous session
            if (session != null)
            {
                try
                {
                    LoadSession(session);
                    debugger = debugger ?? Application.Session.Debugger;
                    architex = architex ?? Application.Session.Architecture.ID;
                }
                catch (Exception x)
                {
                    MessageBox.Show("Error loading session: " + x.ToString());
                }
            }
            // Otherwise, if we've enough info, create a new session
            else if (debugger != null && architex != null)
            {
                NewSession(debugger, architex);
            }
        }

        /// <summary>
        /// Loads the image file at the given path to the current session.
        /// </summary>
        /// <param name="filepath">The path to the image file.</param>
        /// <param name="loadAddress">A new runtime load address or null.</param>
        /// <returns>True when the image is successfully loaded.</returns>
        public static bool LoadImage(string filepath, long? loadAddress = null)
        {
            bool loaded = false;

            foreach (Extension extension in Application.LoadedExtensions)
            {
                try
                {
                    var ext = extension.GetInterface<ILoaderExtension>();

                    // If a valid loader
                    if (ext != null && ext.CanLoad(filepath))
                    {
                        // Load image(s)
                        foreach (var image in ext.LoadResources<ObjectCodeFile>(filepath))
                        {
                            // Rebase as necessary
                            if (loadAddress.HasValue)
                            {
                                image.ActualLoadAddress = loadAddress.Value;
                            }
                            // Include in session
                            Application.Session.IncludeImage(image);
                            loaded = true;
                        }
                        // Don't load more than once
                        break;
                    }
                }
                catch { }
            }
            return loaded;
        }

        /// <summary>
        /// Loads the debugger with the specified name.
        /// </summary>
        /// <param name="name">The name of the debugger to load. Case sensitive.</param>
        /// <returns>True if debugger successfully loaded, false if not found.</returns>
        public static bool LoadDebugger(string name)
        {
            foreach (Extension extension in Application.LoadedExtensions)
            {
                var ext = extension.GetInterface<IDebuggerExtension>();
                if (ext == null) continue;

                // Check if the name matches
                try
                {
                    if (ext.Name != name) continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR] Error accessing debugger name property '{0}': {1}", 
                        extension.Name, e);

                    continue;
                }
                // Load the debugger
                IDebugger newDebugger = ext.LoadDebugger();

                // Assign debugger and propogate change
                Application.Debugger = newDebugger;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Starts a new debug session.
        /// </summary>
        /// <param name="debugger">The name of the session debugger.</param>
        /// <param name="architecture">The name of the session architecture.</param>
        /// <returns>True if successful, false if the given architecture is not found.</returns>
        public static bool NewSession(string debugger, string architecture)
        {
            if (debugger == null)
                throw new ArgumentNullException("debugger");
            if (architecture == null)
                throw new ArgumentNullException("architecture");

            // Query for the given architecture name
            IEnumerable<Architecture> arches = from arch in Application.LoadedArchitectures
                                               where arch.ID == architecture select arch;
            // If not found, report & return
            if (arches.Count() == 0)
            {
                Console.WriteLine("[ERROR] Unknown architecture '" + architecture + "'");
                return false;
            }

            Application.Session = new DebugSession(
                debugger, 
                Application.LoadedExtensions.Select((e) => e.Name).ToArray(),
                arches.Single()
            );
            return true;
        }

        public static void LoadSession(string filename)
        {
            Application.Session = DebugSession.Load(filename);
            
            // Now load images
            var imgs = Application.Session.Properties["DebugOS.LoadedAssemblies"];
            if (imgs != null)
            {
                var split = imgs.Split('?', '*');

                for (int i = 0; i + 1 < split.Length; i += 2)
                {
                    LoadImage(split[i], long.Parse(split[i + 1]));
                }
            }
        }
    }
}
