using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DebugOS.PE
{
    public sealed class DataDirectories
    {
        /// <summary>
        /// The export table address and size.
        /// </summary>
        public DataDirectory? ExportTable { get; private set; }
        /// <summary>
        /// The import table address and size.
        /// </summary>
        public DataDirectory? ImportTable { get; private set; }
        /// <summary>
        /// The resource table address and size.
        /// </summary>
        public DataDirectory? ResourceTable { get; private set; }
        /// <summary>
        /// The exception table address and size.
        /// </summary>
        public DataDirectory? ExceptionTable { get; private set; }
        /// <summary>
        /// The attribute certificate table address and size.
        /// </summary>
        public DataDirectory? CertificateTable { get; private set; }
        /// <summary>
        /// The base relocation table address and size.
        /// </summary>
        public DataDirectory? BaseRelocationTable { get; private set; }
        /// <summary>
        /// The debug data starting address and size.
        /// </summary>
        public DataDirectory? Debug { get; private set; }
        /// <summary>
        /// The Relative Virtual Address of the value to be stored in the 
        /// global pointer register. 
        /// 
        /// The size member of this structure must be set to zero.
        /// </summary>
        public DataDirectory? GlobalPointer { get; private set; }
        /// <summary>
        /// The Thread Local Storage table address and size.
        /// </summary>
        public DataDirectory? TLSTable { get; private set; }
        /// <summary>
        /// The load configuration table address and size.
        /// </summary>
        public DataDirectory? LoadConfigTable { get; private set; }
        /// <summary>
        /// The bound import table address and size.
        /// </summary>
        public DataDirectory? BoundImportTable { get; private set; }
        /// <summary>
        /// The Import Address Table address and size.
        /// </summary>
        public DataDirectory? ImportAddressTable { get; private set; }
        /// <summary>
        /// The delay import descriptor address and size.
        /// </summary>
        public DataDirectory? DelayImportDescriptor { get; private set; }
        /// <summary>
        /// The CLR runtime header address and size.
        /// </summary>
        public DataDirectory? CLRRuntimeHeader { get; private set; }

        public DataDirectories(BinaryReader reader, long directoryCount)
        {
            if (directoryCount == 0) return;

            this.ExportTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 1) return;
            this.ImportTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 2) return;
            this.ResourceTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 3) return;
            this.ExceptionTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 4) return;
            this.CertificateTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 5) return;
            this.BaseRelocationTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 6) return;
            this.Debug = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 7) return;

            reader.ReadUInt64(); // Skip reserved field
            if (directoryCount == 8) return;

            this.GlobalPointer = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 9) return;
            this.TLSTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 10) return;
            this.LoadConfigTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 11) return;
            this.BoundImportTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 12) return;
            this.ImportAddressTable = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 13) return;
            this.DelayImportDescriptor = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 14) return;
            this.CLRRuntimeHeader = new DataDirectory(reader.ReadUInt64());
            if (directoryCount == 15) return;

            reader.ReadUInt64(); // Skip reserved field
        }
    }
}
