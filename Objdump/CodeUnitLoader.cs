/* CodeUnitLoader.cs - (c) James S Renwick 2014
 * -----------------------------------------------
 * Version 1.1.0
 * 
 * This code file contains the logic for parsing and loading
 * code units of an object file from the output produced by objdump.
 * 
 * It ignores all text until it finds the start of a unit,
 * then it loads the code for that unit and repeats until EOF.
 * 
 * TODO: Better error handling
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DebugOS.Loaders
{
    public static class CodeUnitLoader
    {
        // Regex for locating the start of a unit
        static readonly Regex SourceStartRegex
            = new Regex(@"(?<offset>[\da-fA-F]{8}) \<(?<name>.+)\>:");

        // Regex for parsing a line of assembly
        static readonly Regex AssemblyLineRegex
            = new Regex(@" +(?<offset>[\da-fA-F]+):\t(?<bytes>([\da-fA-F]{2} )+)\s*\t(?<opcode>\S+)\s+(?<args>.*)");


        /// <summary>
        /// Loads available code units from an object file.
        /// </summary>
        /// <param name="filepath">The file from which to load code units.</param>
        /// <param name="asmSyntax">The syntax with which to load assembly.</param>
        /// <returns>An array of loaded code units.</returns>
        public static CodeUnit[] Load(string filepath, AssemblySyntax asmSyntax) {
            return Load(new StreamReader(filepath), asmSyntax);
        }

        /// <summary>
        /// Loads available code units from an object file stream.
        /// </summary>
        /// <param name="stream">The stream from which to load code units.</param>
        /// <param name="asmSyntax">The syntax with which to load assembly.</param>
        /// <returns>An array of loaded code units.</returns>
        public static CodeUnit[] Load(StreamReader stream, AssemblySyntax asmSyntax)
        {
            string line;
            var    sources = new List<CodeUnit>();
            
            while ((line = stream.ReadLine()) != null)
            {
                if (String.IsNullOrWhiteSpace(line)) continue;

                // Look for the start of a unit
                var match = SourceStartRegex.Match(line);
                if (match.Success)
                {
                    // Load code for that unit
                    load(stream, line, sources, asmSyntax);
                    return sources.ToArray();
                }
            }
            return null;
        }

        /// <summary>
        /// Parses a line of assembly.
        /// </summary>
        /// <param name="match">The Match containing the line.</param>
        /// <param name="syntax">The code's syntax.</param>
        /// <returns></returns>
        public static AssemblyLine LoadAssembly(Match match, AssemblySyntax syntax)
        {
            uint     offset      = Utils.ParseHex32(match.Groups[2].Value);
            string   instruction = match.Groups[4].Value;
            string[] parameters  = match.Groups[5].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            byte[]   machineCode = new byte[match.Groups[1].Captures.Count];
            string   meta        = "";

            // Remove any metadata or comments
            for (int i = 0; i < parameters.Length; i++)
            {
                int i1 = parameters[i].IndexOf('<');
                int i2 = parameters[i].IndexOf('>');

                if (i1 != -1 && i2 != -1)
                {
                    meta          = parameters[i].Substring(i1, 1 + i2 - i1);
                    parameters[i] = parameters[i].Remove(i1, 1 + i2 - i1);
                }
            }



            int index = 0;
            foreach (Capture item in match.Groups[1].Captures)
            {
                byte b = Utils.ParseHex8(item.Value.Trim());
                machineCode[index++] = b;
            }

            return new AssemblyLine(syntax, offset, instruction, parameters, machineCode, meta);
        }

        private static void load(StreamReader stream, string firstLine, List<CodeUnit> sources, AssemblySyntax syntax)
        {

            Match  match         = SourceStartRegex.Match(firstLine);
            uint   sourceOffset  = Utils.ParseHex32(match.Groups[1].Value);
            uint   currentOffset = sourceOffset;
            string line, name    = match.Groups[2].Value;

            string codeLine = "";
            var    asmLines = new List<AssemblyLine>();
            var    lines    = new List<CodeLine>();

            while ((line = stream.ReadLine()) != null)
            {
                // Line is assembly code
                var asmMatch = AssemblyLineRegex.Match(line);
                if (asmMatch.Success)
                {
                    asmLines.Add(LoadAssembly(asmMatch, syntax));
                    continue;
                }
                // Line indicates next code source - move on to next
                // WARNING: This is BAD - could StackOverflow!
                // TODO: Easy fix!
                var nextSourceMatch = SourceStartRegex.Match(line);
                if (nextSourceMatch.Success)
                {
                    load(stream, line, sources, syntax);
                    break;
                }
                // Line is source code
                if (asmLines.Count != 0) // It's a new line. Save this line & move on to next.
                {
                    int size = asmLines.Sum(asm => asm.MachineCode.Length);

                    lines.Add(new CodeLine(
                        size, currentOffset, codeLine.TrimEnd('\n'), asmLines.ToArray()
                    ));

                    asmLines.Clear();             // Clear temporary list for re-use
                    codeLine       = line;        // Store this new code line
                    currentOffset += (uint)size;  // 
                }
                // Still part of current line
                else { codeLine += '\n' + line; }
            }

            if (asmLines.Count != 0 || !String.IsNullOrWhiteSpace(codeLine.Trim('\n')))
            {
                var size2 = 0;
                foreach (var asmline in asmLines)
                {
                    size2 += asmline.MachineCode.Length;
                }

                lines.Add(new CodeLine(size2, currentOffset, codeLine, asmLines.ToArray()));
            }
            sources.Add(new CodeUnit(sourceOffset, name, lines.ToArray()));
        }
    }
}
