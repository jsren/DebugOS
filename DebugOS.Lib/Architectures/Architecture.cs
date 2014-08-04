using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace DebugOS
{
    [Serializable]
    public class Flag
    {
        public string Initials { get; private set; }
        public string Name { get; private set; }
        public int Offset { get; private set; }
        public int Width { get; private set; }
        public bool CanRead { get; private set; }
        public bool CanWrite { get; private set; }
    }

    [Serializable]
    public class Architecture
    {
        public string ID { get; private set; }
        public int AddressWidth { get; private set; }
        public int StackWidth { get; private set; }
        public StackDirection StackDirection { get; private set; }
        public ByteOrder ByteOrder { get; private set; }
        public string ISA { get; private set; }

        public Register[] Registers { get; private set; }

        /* TODO: Flags */

        private Architecture()
        {

        }
        
        private static int ParseSizeInBits(string size)
        {
            if (String.IsNullOrWhiteSpace(size)) return 0;

            char suffix     = size.Last();
            int  multiplier = 8;

            if (char.IsLetter(suffix))
            {
                size = size.Substring(0, size.Length - 1);

                switch (suffix)
                {
                    case 'b': multiplier = 0x1; break;
                    case 'B': multiplier = 0x8; break;
                    case 'k': multiplier = 0x400; break;
                    case 'K': multiplier = 0x2000; break;
                    case 'm': multiplier = 0x100000; break;
                    case 'M': multiplier = 0x800000; break;
                    default: throw new InvalidDataException("Bad size suffix: '" + suffix + "'");
                }
            }
            return int.Parse(size) * multiplier;
        }

        public static Architecture FromStream(Stream input)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(input);

            if (!string.Equals(doc.DocumentElement.Name, "architecture", 
                StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidDataException("Missing document element 'architecture'");
            }

            string id      = doc.DocumentElement.GetAttribute("id");
            string addrLen = doc.DocumentElement.GetAttribute("address-width");
            string stacLen = doc.DocumentElement.GetAttribute("stack-width");
            string stacDir = doc.DocumentElement.GetAttribute("stack-direction");
            string byteOrd = doc.DocumentElement.GetAttribute("byte-order");
            string isa     = doc.DocumentElement.GetAttribute("isa");

            Architecture output = new Architecture()
            {
                ID             = id,
                AddressWidth   = ParseSizeInBits(addrLen) / 8,
                StackWidth     = ParseSizeInBits(stacLen) / 8,
                StackDirection = (StackDirection)Enum.Parse(typeof(StackDirection), stacDir, true),
                ByteOrder      = (ByteOrder)Enum.Parse(typeof(ByteOrder), byteOrd, true),
                ISA            = isa
            };

            var registers = doc.GetElementsByTagName("register");
            output.Registers = new Register[registers.Count];

            int i = 0;
            foreach (XmlNode node in registers)
            {
                XmlElement element = (XmlElement)node;

                string name  = element.GetAttribute("name");
                string type  = element.GetAttribute("type");
                string width = element.GetAttribute("width");

                output.Registers[i++] = new Register(name, ParseSizeInBits(width) / 8,
                    (RegisterType)Enum.Parse(typeof(RegisterType), type, true));
            }

            return output;
        }

    }
}
