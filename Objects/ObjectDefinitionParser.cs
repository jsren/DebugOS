using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DebugOS.Objects
{
    internal class ObjectDefinitionParser
    {
        XmlDocument doc;

        internal ObjectDefinitionParser(string Filename)
        {
            doc = new XmlDocument();
            doc.Load(Filename);
        }

        public ObjectDefinition[] ParseObjects()
        {
            var objdata = doc.GetElementsByTagName("object");
            var objects = new List<ObjectDefinition>();

            // Load object
            foreach (XmlNode objxml in objdata)
            {
                XmlElement el = objxml as XmlElement;
                if (el == null) continue; // Ignore other nodes

                // Create the necessary format exception:
                var exception = new FormatException("Invalid XML Format");

                // == Parse Attributes ==
                var    fields = new List<FieldDefinition>();
                string name   = el.GetAttribute("name");
                string symbol = el.GetAttribute("symbol");

                if (string.IsNullOrWhiteSpace(name)
                        || string.IsNullOrWhiteSpace(symbol))
                {
                    throw exception;
                }

                foreach (XmlNode fieldxml in el.ChildNodes)
                {
                    XmlElement chel = fieldxml as XmlElement;
                    if (chel == null) continue; // Ignore other nodes

                    // Create the necessary format exception:
                    exception = new FormatException("Invalid XML Format "
                        + "at object '"+name+'\'');

                    // == Parse Attributes ==
                    int width;
                    FieldType type;

                    string swidth = el.GetAttribute("width");
                    string chname = el.GetAttribute("name");
                    string stype  = el.GetAttribute("type");

                    if (string.IsNullOrWhiteSpace(swidth)
                        || string.IsNullOrWhiteSpace(chname)
                        || string.IsNullOrWhiteSpace(stype))
                    {
                        throw exception;
                    }

                    if (!int.TryParse(swidth, out width) ||
                        !Enum.TryParse<FieldType>(stype, out type))
                    {
                        throw exception;
                    }

                    fields.Add(new FieldDefinition(chname, width, type));
                }

                objects.Add(new ObjectDefinition(name, symbol, fields.ToArray()));
            }
            return objects.ToArray();
        }
    }
}
