using System;
using System.Collections.Generic;
using System.IO;

namespace DebugOS.Objects
{
    public sealed class MemoryObject
    {
        private Dictionary<string, string> dict;

        private MemoryObject()
        {
            dict = new Dictionary<string, string>();
        }

        public MemoryObject(byte[] Data, ObjectDefinition ObjectDefinition) : this()
        {
            int index = 0;
            foreach (FieldDefinition fieldDef in ObjectDefinition.Fields)
            {
                string value = null;
                if (fieldDef.Type == FieldType.Padding) continue;

                switch (fieldDef.Type)
                {
                    case FieldType.Integer:
                    {
                        value = LoadInteger(Data, index, fieldDef.Width).ToString();
                        break;
                    }
                }
                dict.Add(fieldDef.Name, value);
            }
        }

        private ulong LoadInteger(byte[] data, int index, int bits)
        {
            ulong output;
            ulong bitmask = 1;

            for (int i = 1; i < bits; i++) bitmask *= 2;
            bitmask -= 1;

            unsafe
            {
                fixed (byte* bptr = data)
                {
                    ulong* sptr = (ulong*)(bptr + index);
                    output = *sptr;
                }
            }
            return output & bitmask;
        }
    }
}
