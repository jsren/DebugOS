using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DebugOS.GDB.Formats
{
    public static class arm11
    {
        private static byte[] ParseValue(StringReader reader, int width)
        {
            byte[] output = new byte[width];

            for (int i = 0; i < width; i++)
            {
                string hex = (char)reader.Read() + "" + (char)reader.Read();
                output[i] = Utils.ParseHex8(hex);
            }
            return output;
        }

        public static Dictionary<Register, byte[]> Parse(string data)
        {
            var output = new Dictionary<Register, byte[]>();

            using (StringReader reader = new StringReader(data))
            {
                output.Add(new Register("R0", 4), ParseValue(reader, 4));
                output.Add(new Register("R1", 4), ParseValue(reader, 4));
                output.Add(new Register("R2", 4), ParseValue(reader, 4));
                output.Add(new Register("R3", 4), ParseValue(reader, 4));
                output.Add(new Register("R4", 4), ParseValue(reader, 4));
                output.Add(new Register("R5", 4), ParseValue(reader, 4));
                output.Add(new Register("R6", 4), ParseValue(reader, 4));
                output.Add(new Register("R7", 4), ParseValue(reader, 4));
                output.Add(new Register("R8", 4), ParseValue(reader, 4));
                output.Add(new Register("R9", 4), ParseValue(reader, 4));
                output.Add(new Register("R10", 4), ParseValue(reader, 4));
                output.Add(new Register("R11", 4), ParseValue(reader, 4));
                output.Add(new Register("R12", 4), ParseValue(reader, 4));

                output.Add(new Register("R13", 4, RegisterType.StackPointer), ParseValue(reader, 4));
                output.Add(new Register("R14", 4, RegisterType.ReturnAddress), ParseValue(reader, 4));
                output.Add(new Register("R15", 4, RegisterType.InstructionPointer), ParseValue(reader, 4));

                /* TODO: FPAs and CPSR */

            }
            return output;
        }


    }
}
