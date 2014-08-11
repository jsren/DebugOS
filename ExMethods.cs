
namespace DebugOS
{
    public static class ExtensionMethods
    {
        public static int Read(this System.IO.Stream stream, byte[] buffer)
        {
            return stream.Read(buffer, 0, buffer.Length);
        }

        public static void Write(this System.IO.Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
