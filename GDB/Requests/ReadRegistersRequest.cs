using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.GDB
{
    public sealed class ReadRegistersRequest : Request<Dictionary<Register,byte[]>>
    {
        public ReadRegistersRequest(Message msg) : base(msg)
        {

        }

        public ReadRegistersRequest(Action<Request> callback) : base(callback)
        {

        }

        protected override Dictionary<Register, byte[]> GetResult(Message response)
        {
            if (!this.HasResponseData)
            {
                throw new InvalidOperationException(
                    "GDB::ReadRegistersRequest malformed - requires response data");
            }
            else if (response.IsError)
            {
                throw new Exception(String.Format("An error occurred reading register values from GDB: 0x{0:X}",
                    response.ErrorCode));
            }

            /* ARCHITECTURE-SPECIFIC STUFF HERE */

            if (Application.Session.Architecture.ISA == "x86")
            {
                return Formats.Regsi386.Parse(response.Data); 
            }
            else if (Application.Session.Architecture.ISA == "ARM")
            {
                return Formats.arm11.Parse(response.Data);
            }
            else
            {
                Console.WriteLine("[ERROR] Unknown architecture for GDB::ReadRegisters: '{0}'",
                    Application.Session.Architecture.ISA);

                return new Dictionary<Register, byte[]>();
            }
        }
    }
}
