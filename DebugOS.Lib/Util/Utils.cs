using System;
using System.Net;
using NumberStyles = System.Globalization.NumberStyles;

namespace DebugOS
{
    public static partial class Utils
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

        public static string SanitizeHex(string hexString)
        {
            // Remove underscores
            hexString = hexString.Replace("_", "");
            // Remove spaces
            hexString = hexString.Replace(" ", "");

            // Remove hex prefix
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString.Remove(0, 2);
            }
            
            // Make all uppercase
            return hexString.ToUpper();
        }

        public static byte ParseHex8(string hexString)
        {
            return byte.Parse(SanitizeHex(hexString), hexNo);
        }

        public static uint ParseHex32(string hexString)
        {
            return uint.Parse(SanitizeHex(hexString), hexNo);
        }

        public static ulong ParseHex64(string hexString)
        {
            return ulong.Parse(SanitizeHex(hexString), hexNo);
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

        public static uint SwitchByteOrder(uint value)
        {
            byte[] data = IntToBytes((int)value);
            Array.Reverse(data);
            return (uint)LongFromBytes(data);
        }

        public static byte[] IntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static int IntFromBytes(byte[] data)
        {
            if (data.Length < sizeof(int))
            {
                byte[] copy = new byte[sizeof(int)];

                if (Application.Session.Architecture.ByteOrder == ByteOrder.LittleEndian)
                {
                    Array.Copy(data, copy, data.Length);
                    data = copy;
                }
                else
                {
                    Array.Copy(data, 0, copy, sizeof(int) - data.Length, data.Length);
                    data = copy;
                }
            }
            return BitConverter.ToInt32(data, 0);
        }

        public static long LongFromBytes(byte[] data)
        {
            if (data.Length < sizeof(long))
            {
                byte[] copy = new byte[sizeof(long)];

                if (Application.Session.Architecture.ByteOrder == ByteOrder.LittleEndian)
                {
                    Array.Copy(data, copy, data.Length);
                    data = copy;
                }
                else
                {
                    Array.Copy(data, 0, copy, sizeof(long) - data.Length, data.Length);
                    data = copy;
                }
            }
            return BitConverter.ToInt64(data, 0);
        }

        public static int Pow(int p1, int p2)
        {
            // If less than zero, use fp version
            if (p2 < 0)
            {
                return (int)Math.Pow(p1, p2);
            }
            // Otherwise, just multiply
            else
            {
                // Initial value of 1
                int output = 1;
                // Multiply
                for (int i = 0; i < p2; i++) {  output *= p1; }
                // Return
                return output;
            }
        }

        public static string GetPlatformPath(string path)
        {
            const string cygwinStart = "/cygdrive/";

            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }
            else
            {
                if (path.StartsWith(cygwinStart, StringComparison.InvariantCultureIgnoreCase))
                {
                    path = path.Remove(0, cygwinStart.Length);
                    path = path.Insert(1, ":");
                }
                return System.IO.Path.GetFullPath(path);
            }
        }
    }
}
