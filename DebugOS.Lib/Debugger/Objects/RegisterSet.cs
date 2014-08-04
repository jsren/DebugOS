using System;
using System.Collections.Generic;

namespace DebugOS
{
    public class RegisterSet
    {
        protected List<Register> registers;
        protected List<Tuple<bool, bool>> access;


        public virtual Register[] Registers
        { 
            get { return this.registers.ToArray(); } 
        }

        public virtual Register this[string registerName]
        {
            get
            {
                foreach (Register reg in this.registers)
                {
                    if (StringComparer.CurrentCultureIgnoreCase
                        .Equals(reg.Name, registerName))
                    {
                        return reg;
                    }
                }
                return null;
            }
        }


        protected RegisterSet()
        {
            this.access    = new List<Tuple<bool, bool>>();
            this.registers = new List<Register>();
        }

        public RegisterSet(Register[] registers, bool[] readableArray, bool[] writeableArray) 
            : this()
        {
            // Assert that arrays are the same length
            if (registers.Length != readableArray.Length || 
                registers.Length != writeableArray.Length)
            {
                throw new ArgumentException("Input array lengths do not match");
            }

            // Add registers
            this.registers.AddRange(registers);

            // Add accessibility
            for (int i = 0; i < registers.Length; i++)
            {
                this.access.Add(new Tuple<bool,bool>(readableArray[i], writeableArray[i]));
            }
        }

        public virtual bool HasRegister(string registerName)
        {
            return this[registerName] != null;
        }

        public virtual bool CanRead(string registerName)
        {
            Register reg = this[registerName];

            if (reg == null) return false;
            else return this.access[this.registers.IndexOf(reg)].Item1;
        }

        public virtual bool CanWrite(string registerName)
        {
            Register reg = this[registerName];

            if (reg == null) return false;
            else return this.access[this.registers.IndexOf(reg)].Item2;
        }
    }
}
