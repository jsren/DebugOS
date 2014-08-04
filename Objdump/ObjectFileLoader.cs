﻿/* ObjectFileLoader.cs - (c) James S Renwick 2014
 * -----------------------------------------------
 * Version 1.2.0
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

namespace DebugOS.Loaders
{
    /// <summary>
    /// Helper class facilitating object file loading.
    /// </summary>
    public static class ObjectFileLoader
    {
        // Regexes for parsing the architecture & start address of the object file
        static readonly Regex ArchRegex = new Regex(@"architecture: (?<arch>.+),.*");
        static readonly Regex AddrRegex = new Regex(@"start address 0x(?<addr>[\da-fA-F]+).*");

        /// <summary>
        /// Creates an ObjectCodeFile object from a object file.
        /// </summary>
        /// <param name="filepath">The path to the object file.</param>
        /// <param name="asmSyntax">The syntax of the assembly to load.</param>
        /// <returns>An ObjectCodeFile for the given object file.</returns>
        public static ObjectCodeFile Load(string filepath, AssemblySyntax asmSyntax)
        {
            string argAdd = "";

            if (asmSyntax == AssemblySyntax.Intel) {
                argAdd += " -M intel";
            }

            // Look for a given objdump binary path
            string exePath = Application.Arguments["-objdump"];

            if (exePath == null && Application.Session != null)
            {
                exePath = Application.Session.Properties["Objdump.Path"];
            }
            // finally just invoke directly
            exePath = Path.GetFullPath(exePath ?? "objdump.exe");

            // Try to invoke objdump
            var proc = Process.Start(new ProcessStartInfo()
            {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                FileName               = exePath,
                WorkingDirectory       = Path.GetDirectoryName(exePath),
                Arguments = "-x -l -S -C" + argAdd + " \"" + filepath + '"'
            });

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
