using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DebugOS.GDB
{
    /// <summary>
    /// An ASCII message sent via the GDB protocol.
    /// </summary>
    public class Message
    {
        private const char StartChar    = '$';
        private const char ChecksumChar = '#';
        private const byte EscapeChar   = 0x7D;
        private const byte EscapeTrans  = 0x20;

        private static readonly Regex errorRegex = new Regex(@"^E *([\da-zA-Z][\da-zA-Z]?)$");

        /// <summary>
        /// Gets the string contents of the message.
        /// </summary>
        public string Data { get; protected set; }

        /// <summary>
        /// Whether the message is an 'OK' response.
        /// </summary>
        public bool IsOK { get; protected set; }
        /// <summary>
        /// Whether the message is an error response.
        /// </summary>
        public bool IsError { get; protected set; }
        /// <summary>
        /// The message's error code, if applicable.
        /// </summary>
        public int ErrorCode { get; protected set; }


        /// <summary>
        /// Constructs a message object from raw message data.
        /// </summary>
        /// <param name="data">The buffer containing the message.</param>
        /// <param name="startIndex">The index at which the message starts.</param>
        /// <param name="length">The length of the message.</param>
        public Message(byte[] data, int startIndex, int length)
        {
            if (data[startIndex] != StartChar || length < 4)   throw new FormatException();
            if (data[startIndex + length - 3] != ChecksumChar) throw new FormatException();

            byte checksum = Convert.ToByte(ASCIIEncoding.ASCII.GetString(data, startIndex + length - 2, 2), 16);

            byte sum = 0;
            for (int i = startIndex + 1; i < startIndex + length - 3; i++)
            {
                sum += data[i];
            }
            if (checksum != sum) throw new FormatException("Invalid checksum");

            this.Data = unescape(ASCIIEncoding.ASCII.GetString(data, startIndex + 1, length - 4));
            this.Interpret();
        }
        /// <summary>
        /// Creates a new message from its string contents.
        /// </summary>
        /// <param name="data">The contents of the message.</param>
        public Message(string data)
        {
            this.Data = data ?? "";
            this.Interpret();
        }

        /// <summary>
        /// Gets the raw message bytes for transmission.
        /// </summary>
        /// <returns></returns>
        public byte[] GetMessageBytes()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(StartChar);

            string msg = escape(this.Data);

            byte sum = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                sum += (byte)msg[i];
            }

            builder.Append(msg);
            builder.AppendFormat("#{0:X}", sum);

            return ASCIIEncoding.ASCII.GetBytes(builder.ToString());
        }

        private void Interpret()
        {
            if (this.Data == "OK")
            {
                this.IsOK = true;
            }
            else
            {
                Match match = errorRegex.Match(this.Data);
                if (match.Success)
                {
                    this.IsError = true;
                    this.ErrorCode = Convert.ToInt32(match.Groups[1].Value, 16);
                }
            }
        }


        private string escape(string msg)
        {
            return msg;
        }

        private string unescape(string msg)
        {
            return msg;
        }
    }
}
