using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class Header
    {
        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<Header>();
        public IDictionary<string, string> Values = new Dictionary<string, string>();

        public bool Parse(byte[] buffer, int size, ref string error_msg)
        {
            int i = 0;
            while (i < size)
            {
                int thispiece = BitConverter.ToInt32(buffer, i);
                i += 4;
                byte[] line = new byte[thispiece];
                Array.Copy(buffer, i, line, 0, thispiece);
                string thisheader = Encoding.ASCII.GetString(line);
                string[] chunks = thisheader.Split('=');
                if (chunks.Length != 2)
                {
                    i += thispiece;
                    continue;
                }
                Values[chunks[0].Trim()] = chunks[1].Trim();
                i += thispiece;
            }

            if (i != size)
            {
                Logger.LogWarning("Could not parse connection header.");
                return false;
            }

            return true;
        }

        public void Write(IDictionary<string, string> dict, out byte[] buffer, out int totallength)
        {
            var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, Encoding.ASCII))
            {
                foreach (string k in dict.Keys)
                {
                    byte[] key = Encoding.ASCII.GetBytes(k);
                    byte[] val = Encoding.ASCII.GetBytes(dict[k]);
                    int lineLength = val.Length + key.Length + 1;

                    writer.Write(lineLength);
                    writer.Write(key);
                    writer.Write('=');
                    writer.Write(val);
                }
            }

            ms.TryGetBuffer(out ArraySegment<byte> result);
            buffer = new byte[result.Count];
            Array.Copy(result.Array, result.Offset, buffer, 0, result.Count);
            totallength = result.Count;
        }

        public static byte[] ByteLength(int num)
        {
            return ByteLength((uint) num);
        }

        public static byte[] ByteLength(uint num)
        {
            return BitConverter.GetBytes(num);
        }
    }
}
