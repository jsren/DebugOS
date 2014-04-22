using System;
using System.Linq;

namespace DebugOS
{
    /// <summary>
    /// Object representing a single line of higher-level code.
    /// </summary>
    public sealed class CodeLine
    {
        /// <summary>The size in bytes of the line.</summary>
        public int Size { get; private set; }
        /// <summary>The offset of the line within the object file.</summary>
        public uint Offset { get; private set; }
        /// <summary>The text code of the line.</summary>
        public String Text { get; private set; }
        /// <summary>The assembly instructions for the line.</summary>
        public AssemblyLine[] Assembly { get; private set; }

        /// <summary>
        /// Creates a new code line object.
        /// </summary>
        /// <param name="Size">The size in bytes of the line.</param>
        /// <param name="Offset">The offset of the line within the object file.</param>
        /// <param name="Text">The text code of the line.</param>
        /// <param name="Assembly">The assembly instructions for the line.</param>
        public CodeLine(int Size, uint Offset, String Text, AssemblyLine[] Assembly)
        {
            this.Size     = Size;
            this.Offset   = Offset;
            this.Text     = Text;
            this.Assembly = Assembly;
        }

        /// <summary>
        /// Gets the string source of the code line.
        /// </summary>
        /// <returns>A string containing the source code of the line.</returns>
        public override string ToString() {
            return Text;
        }

        /// <summary>
        /// Gets the string assembly of the code line.
        /// </summary>
        /// <returns>A string containing the assembly code of the line.</returns>
        public string GetAssemblyString() {
            return string.Join("\n", this.Assembly.Select(asm => asm.ToString()));
        }

        /// <summary>
        /// Gets the line of assembly at the specified address.
        /// If not found, returns null.
        /// </summary>
        /// <param name="Address">The address to get the assembly from.</param>
        /// <returns>The assembly at the specified address, or null.</returns>
        public AssemblyLine GetAssembly(uint Address)
        {
            for (int i = 0; i < Assembly.Length; i++)
            {
                AssemblyLine a = Assembly[i];

                if (a.Offset <= Address && a.Offset + a.MachineCode.Length > Address) {
                    return a;
                }
            }
            return null;
        }
    }
}
