/* ObjectFileLoader.cs - (c) James S Renwick 2014
 * -----------------------------------------------
 * Version 1.3.0
 * 
 * This code file contains the logic for creating an ObjectCodeFile object
 * from the output produced by objdump.
 * 
 * It searches for the architecture and start address. Once located
 * (or the stream is exhausted), loads the symbol table and code.
 * 
 * TODO: Better error handling
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace DebugOS.Loaders.Objdump
{
    /// <summary>
    /// Helper class facilitating object file loading.
    /// </summary>
    public sealed class ObjectFileLoader : ILoaderExtension
    {
        // Regexes for parsing the architecture & start address of the object file
        static readonly Regex ArchRegex = new Regex(@"architecture: (?<arch>.+),.*");
        static readonly Regex AddrRegex = new Regex(@"start address 0x(?<addr>[\da-fA-F]+).*");


        private string objdumpPath;

        /// <summary>
        /// Gets the loader name.
        /// </summary>
        public string Name
        {
            get { return "Objdump Debug Info Loader"; }
        }

        /// <summary>
        /// Initialises the loader.
        /// </summary>
        public void Initialise(string[] args)
        {
            // Look for a given objdump binary path
            objdumpPath = Application.Arguments["-objdump"];

            if (objdumpPath == null && Application.Session != null)
            {
                objdumpPath = Application.Session.Properties["Objdump.Path"];
            }
            // If none found, just invoke directly
            objdumpPath = Path.GetFullPath(objdumpPath ?? "objdump.exe");
        }

        /// <summary>
        /// Tests whether the loader accepts the given file.
        /// </summary>
        /// <param name="filepath">The path to the file to load.</param>
        /// <returns>True if the loader will accept the file, false otherwise.</returns>
        public bool CanLoad(string filepath)
        {
            try
            {
                // Try to invoke objdump
                var proc = Process.Start(new ProcessStartInfo()
                {
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    FileName               = objdumpPath,
                    WorkingDirectory       = Path.GetDirectoryName(objdumpPath),
                    Arguments = "-f \"" + filepath + '"'
                });
                if (!proc.WaitForExit(3000) || proc.ExitCode != 0) throw new Exception();
            }
            catch (Exception) { return false; }

            return true;
        }

        public IDebugResource[] LoadResources(string filepath)
        {
            AssemblySyntax syntax = 0;

            if (Application.Session != null)
            {
                Enum.TryParse<AssemblySyntax>(
                    Application.Session.Properties["Settings::Code::AssemblySyntax"],
                    true, 
                    out syntax
                );
            }
            return new IDebugResource[] { this.Load(filepath, syntax) };
        }


        public T[] LoadResources<T>(string filepath) where T : IDebugResource
        {
            // Just load object code file(s)
            if (typeof(T) == typeof(ObjectCodeFile))
            {
                return this.LoadResources(filepath).Cast<T>().ToArray();
            }
            // Load object code file(s) (& others) and get the resulting code units
            else if (typeof(T) == typeof(CodeUnit))
            {
                var output = new List<CodeUnit>();

                foreach (IDebugResource res in this.LoadResources(filepath))
                {
                    if (res.GetType() == typeof(ObjectCodeFile))
                    {
                        output.AddRange(((ObjectCodeFile)res).Code);
                    }
                    else if (res.GetType() == typeof(CodeUnit))
                    {
                        output.Add((CodeUnit)res);
                    }
                }
                return output.Cast<T>().ToArray();
            }
            // Load object code file(s) (& others) and get the resulting symbol tables
            else if (typeof(T) == typeof(SymbolTable))
            {
                var output = new List<SymbolTable>();

                foreach (IDebugResource res in this.LoadResources(filepath))
                {
                    if (res.GetType() == typeof(ObjectCodeFile))
                    {
                        output.Add(((ObjectCodeFile)res).SymbolTable);
                    }
                    else if (res.GetType() == typeof(SymbolTable))
                    {
                        output.Add((SymbolTable)res);
                    }
                }
                return output.Cast<T>().ToArray();
            }
            // We don't support loading this type - just return empty
            else return new T[0];
        }

        /// <summary>
        /// Creates an ObjectCodeFile object from a object file.
        /// </summary>
        /// <param name="filepath">The path to the object file.</param>
        /// <param name="asmSyntax">The syntax of the assembly to load.</param>
        /// <returns>An ObjectCodeFile for the given object file.</returns>
        public ObjectCodeFile Load(string filepath, AssemblySyntax asmSyntax)
        {
            string argAdd = "";

            if (asmSyntax == AssemblySyntax.Intel) {
                argAdd += " -M intel";
            }

            // Try to invoke objdump
            var procInfo = new ProcessStartInfo()
            {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                FileName               = objdumpPath,
                WorkingDirectory       = Path.GetDirectoryName(objdumpPath),
                Arguments = "-x -l -S -C" + argAdd + " \"" + filepath + '"'
            };

            using (Process proc = Process.Start(procInfo))
            {
                // Load from objdump output
                var output = LoadFromObjdump(proc.StandardOutput, Path.GetFullPath(filepath), asmSyntax);

                // Check for error
                if (proc.WaitForExit(3000) && proc.ExitCode != 0)
                {
                    throw new Exception(String.Format("objdump returned error code {0}: {1}",
                        proc.ExitCode, proc.StandardError.ReadToEnd()));
                }
                else return output;
            }
        }

        /// <summary>
        /// Creates an ObjectCodeFile object from the output stream of objdump.
        /// </summary>
        /// <param name="stream">The objdump output stream.</param>
        /// <param name="filename">The name of the object code file.</param>
        /// <param name="asmSyntax">The syntax of the assembly to load.</param>
        /// <returns>An ObjectCodeFile from the given stream.</returns>
        private static ObjectCodeFile LoadFromObjdump(StreamReader stream, string filename, AssemblySyntax asmSyntax)
        {
            // Count the matched regexes
            const int find = 2;
            int findCount  = 0;

            string line;
            string architecture = "";
            uint   startAddress = 0;

            // Load 
            while ((line = stream.ReadLine()) != null && findCount < find)
            {
                if (String.IsNullOrWhiteSpace(line)) continue;

                Match match;

                // Match for architecture
                if ((match = ArchRegex.Match(line)).Success)
                { 
                    architecture = match.Groups[1].Value;
                    findCount++;
                    continue;
                }
                // Match for start address
                if ((match = AddrRegex.Match(line)).Success)
                {
                    startAddress = Utils.ParseHex32(match.Groups[1].Value);
                    findCount++;
                    continue;
                }
            }

            // Load the symbol table and code
            // DO NOT RE-ORDER THESE CALLS
            Section[]   secs  = SectionsLoader.Load(stream);
            SymbolTable table = SymbolTableLoader.LoadFromObjdump(stream);
            CodeUnit[]  code  = CodeUnitLoader.Load(stream, asmSyntax);

            var unfound = new System.Collections.Generic.List<string>();

            foreach (CodeUnit unit in code)
            {
                bool present = false;
                foreach (SymbolEntry entry in table)
                {
                    if (unit.Name == entry.Name)
                    {
                        present = true;
                        break;
                    }
                }
                if (!present) unfound.Add(unit.Name);
            }

            return new ObjectCodeFile(startAddress, filename, architecture, secs, table, code);
        }
    }
}
