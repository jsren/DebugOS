using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.Bochs
{
    /*public sealed class BochsRegSet : RegisterSet
    {
        private static readonly string[] regs = new string[]
        {
            "ax", "bp", "bx", "cx", "di", "dx", "ip", "si", "sp"
        };

        private Dictionary<Register, byte[]> registers;


        public BochsRegSet()
        {
            this.registers = new Dictionary<Register, byte[]>();
        }


        internal void UpdateRegister(string name, byte[] value)
        {

        }

        public override Register this[string registerName]
        {
            get
            {
                registerName = registerName.ToLower();

                // Look for valid overloaded register
                if (regs.Where((reg) => registerName.EndsWith(reg)).Any())
                {
                    if (registerName.Length == 3)
                    {
                        if (registerName[0] == 'r')
                        {
                            return new Register(registerName.ToUpper(), 
                        }
                    }
                    else if (registerName.Length == 2) // 16-bit
                    {
                        return base[
                    }
                }
                else return base[registerName];
            }
        }

    }*/
}
