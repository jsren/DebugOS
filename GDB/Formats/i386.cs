using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DebugOS.GDB.Formats
{
    public static class Regsi386
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
                output.Add(new Register("EAX", 4), ParseValue(reader, 4));
                output.Add(new Register("ECX", 4), ParseValue(reader, 4));
                output.Add(new Register("EDX", 4), ParseValue(reader, 4));
                output.Add(new Register("EBX", 4), ParseValue(reader, 4));
                output.Add(new Register("ESP", 4, RegisterType.StackPointer), ParseValue(reader, 4));
                output.Add(new Register("EBP", 4, RegisterType.FramePointer), ParseValue(reader, 4));
                output.Add(new Register("ESI", 4), ParseValue(reader, 4));
                output.Add(new Register("EDI", 4), ParseValue(reader, 4));
                output.Add(new Register("EIP", 4, RegisterType.InstructionPointer), ParseValue(reader, 4));
                output.Add(new Register("EFLAGS", 4, RegisterType.Flags), ParseValue(reader, 4));
                output.Add(new Register("CS", 4, RegisterType.Segment), ParseValue(reader, 4));
                output.Add(new Register("SS", 4, RegisterType.Segment), ParseValue(reader, 4));
                output.Add(new Register("DS", 4, RegisterType.Segment), ParseValue(reader, 4));
                output.Add(new Register("ES", 4, RegisterType.Segment), ParseValue(reader, 4));
                output.Add(new Register("FS", 4, RegisterType.Segment), ParseValue(reader, 4));
                output.Add(new Register("GS", 4, RegisterType.Segment), ParseValue(reader, 4));

                /* Floating-point registers */

                output.Add(new Register("ST0", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST1", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST2", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST3", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST4", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST5", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST6", 10, RegisterType.Extended), ParseValue(reader, 10));
                output.Add(new Register("ST7", 10, RegisterType.Extended), ParseValue(reader, 10));

                /* TODO: Floating-point control registers */

            }
            return output;
        }

    }
}
