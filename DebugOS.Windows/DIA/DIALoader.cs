using System;
using System.IO;
using System.Runtime.InteropServices;
using diatypes;

namespace DebugOS.Windows
{
    [ComImport, Guid("761D3BCD-1304-41D5-94E8-EAC54E4AC172")]
    class DiaDataSource { }

    public static class DIALoader
    {
        private const int E_PDB_NOT_FOUND = unchecked((int)0x806D0005);

        public static bool DIASupported { get; private set; }

        static DIALoader()
        {
            // Check that COM classes are loaded and thus DIA is supported
            try
            {
                ((IDiaDataSource)new DiaDataSource()).ToString();
                DIASupported = true;
            }
            catch (COMException) { }
        }

        public static IDiaSession LoadFromPDB(string filepath)
        {
            IDiaSession output;

            var dataSource = (IDiaDataSource)new DiaDataSource();

            dataSource.loadDataFromPdb(filepath);
            dataSource.openSession(out output);

            return output;
        }

        public static IDiaSession LoadFromEXE(string filepath)
        {
            IDiaSession output;

            var dataSource = (IDiaDataSource)new DiaDataSource();

            try
            {
                dataSource.loadDataForExe(filepath, "", null);
            }
            catch (COMException e)
            {
                if (e.ErrorCode == E_PDB_NOT_FOUND)
                {
                    throw new FileNotFoundException("Failed to open the file, "
                        + "or the file has an invalid format.");
                }
                else throw;
            }
            dataSource.openSession(out output);
            return output;
        }

    }
}
