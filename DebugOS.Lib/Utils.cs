using System;
using System.Net;
using NumberStyles = System.Globalization.NumberStyles;

namespace DebugOS
{
    public static class Utils
    {
        static readonly NumberStyles hexNo = NumberStyles.HexNumber;

        public static string GetHexString(uint Address, int fixedPlaces = 0, bool prefix = false)
        {
            if (fixedPlaces < 0) throw new ArgumentOutOfRangeException();
            if (fixedPlaces == 0)
            {
                return (prefix ? "0x" : "") + string.Format("{0:X}", Address);
            }
            else return (prefix ? "0x" : "") + string.Format("{0:X" + fixedPlaces.ToString() + "}", Address);
        }

        public static string GetHexString(ulong Address, int fixedPlaces = 0, bool prefix = false)
        {
            if (fixedPlaces < 0) throw new ArgumentOutOfRangeException();
            if (fixedPlaces == 0)
            {
                return (prefix ? "0x" : "") + string.Format("{0:X}", Address);
            }
            else return (prefix ? "0x" : "") + string.Format("{0:X" + fixedPlaces.ToString() + "}", Address);
        }

        public static byte ParseHex8(string HexString)
        {
            if (HexString.StartsWith("0x"))
                HexString = HexString.Remove(0, 2);

            return byte.Parse(HexString, hexNo);
        }

        public static uint ParseHex32(string HexString)
        {
            if (HexString.StartsWith("0x"))
                HexString = HexString.Remove(0, 2);

            return uint.Parse(HexString, hexNo);
        }

        public static ulong ParseHex64(string HexString)
        {
            if (HexString.StartsWith("0x"))
                HexString = HexString.Remove(0, 2);

            return ulong.Parse(HexString, hexNo);
        }

        public static uint DWordAlign(uint Address, bool AlignUp = true)
        {
            Address += Address % sizeof(uint);
            return AlignUp ? Address + sizeof(uint) : Address;
        }
        public static ulong QWordAlign(ulong Address, bool AlignUp = true)
        {
            Address += Address % sizeof(ulong);
            return AlignUp ? Address + sizeof(ulong) : Address;
        }

        public static uint MakeLittleEndian(uint BigEndian)
        {
            unchecked { return (uint)IPAddress.HostToNetworkOrder((int)BigEndian); }
        }

        public static int IntFromBytes(byte[] data)
        {
            return System.Runtime.InteropServices.Marshal.ReadInt32(data, 0);
        }

        public static long LongFromBytes(byte[] data)
        {
            return System.Runtime.InteropServices.Marshal.ReadInt64(data, 0);
        }

        public static int Pow(int p1, int p2)
        {
            if (p2 < 0) return (int)Math.Pow(p1, p2);

            int output = 1;
            for (int i = 0; i < p2; i++) { output *= p1; }
            return output;
        }
    }
}
