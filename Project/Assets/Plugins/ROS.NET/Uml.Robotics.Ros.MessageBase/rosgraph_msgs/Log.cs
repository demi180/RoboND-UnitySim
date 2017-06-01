using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;

using Messages.std_msgs;

namespace Messages.rosgraph_msgs
{
    public class Log : RosMessage
    {
        public const byte DEBUG = 1;
        public const byte INFO = 2;
        public const byte WARN = 4;
        public const byte ERROR = 8;
        public const byte FATAL = 16;

        public Header header = new Header();
        public byte level = new byte();
        public string name = "";
        public string msg = "";
        public string file = "";
        public string function = "";
        public uint line = new uint();
        public string[] topics;

        public override string MD5Sum() { return "acffd30cd6b6de30f120938c17c593fb"; }
        public override bool HasHeader() { return true; }
        public override bool IsMetaType() { return true; }
        public override string MessageDefinition() { return @"byte DEBUG=1
byte INFO=2
byte WARN=4
byte ERROR=8
byte FATAL=16
Header header
byte level
string name
string msg
string file
string function
uint32 line
string[] topics"; }
        public override string MessageType { get { return "rosgraph_msgs/Log"; } }
        public override bool IsServiceComponent() { return false; }

        public Log()
        {
        }

        public Log(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public Log(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            int arraylength = -1;
            bool hasmetacomponents = false;
            int piecesize = 0;
            IntPtr h;

            //header
            header = new Header(serializedMessage, ref currentIndex);
            //level
            level=serializedMessage[currentIndex++];
            //name
            name = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            name = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
            //msg
            msg = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            msg = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
            //file
            file = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            file = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
            //function
            function = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            function = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
            //line
            piecesize = Marshal.SizeOf(typeof(uint));
            h = IntPtr.Zero;
            if (serializedMessage.Length - currentIndex != 0)
            {
                h = Marshal.AllocHGlobal(piecesize);
                Marshal.Copy(serializedMessage, currentIndex, h, piecesize);
            }
            if (h == IntPtr.Zero)
                throw new Exception("Memory allocation failed");
            line = (uint)Marshal.PtrToStructure(h, typeof(uint));
            Marshal.FreeHGlobal(h);
            currentIndex+= piecesize;
            //topics
            hasmetacomponents |= false;
            arraylength = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += Marshal.SizeOf(typeof(System.Int32));
            if (topics == null)
                topics = new string[arraylength];
            else
                Array.Resize(ref topics, arraylength);
            for (int i=0;i<topics.Length; i++) {
                //topics[i]
                topics[i] = "";
                piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
                currentIndex += 4;
                topics[i] = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
                currentIndex += piecesize;
            }
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            bool hasmetacomponents = false;
            byte[] thischunk, scratch1, scratch2;
            List<byte[]> pieces = new List<byte[]>();
            GCHandle h;

            //header
            if (header == null)
                header = new Header();
            pieces.Add(header.Serialize(true));
            //level
            pieces.Add(new[] { (byte)level });
            //name
            if (name == null)
                name = "";
            scratch1 = Encoding.ASCII.GetBytes((string)name);
            thischunk = new byte[scratch1.Length + 4];
            scratch2 = BitConverter.GetBytes(scratch1.Length);
            Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
            Array.Copy(scratch2, thischunk, 4);
            pieces.Add(thischunk);
            //msg
            if (msg == null)
                msg = "";
            scratch1 = Encoding.ASCII.GetBytes((string)msg);
            thischunk = new byte[scratch1.Length + 4];
            scratch2 = BitConverter.GetBytes(scratch1.Length);
            Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
            Array.Copy(scratch2, thischunk, 4);
            pieces.Add(thischunk);
            //file
            if (file == null)
                file = "";
            scratch1 = Encoding.ASCII.GetBytes((string)file);
            thischunk = new byte[scratch1.Length + 4];
            scratch2 = BitConverter.GetBytes(scratch1.Length);
            Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
            Array.Copy(scratch2, thischunk, 4);
            pieces.Add(thischunk);
            //function
            if (function == null)
                function = "";
            scratch1 = Encoding.ASCII.GetBytes((string)function);
            thischunk = new byte[scratch1.Length + 4];
            scratch2 = BitConverter.GetBytes(scratch1.Length);
            Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
            Array.Copy(scratch2, thischunk, 4);
            pieces.Add(thischunk);
            //line
            scratch1 = new byte[Marshal.SizeOf(typeof(uint))];
            h = GCHandle.Alloc(scratch1, GCHandleType.Pinned);
            Marshal.StructureToPtr(line, h.AddrOfPinnedObject(), false);
            h.Free();
            pieces.Add(scratch1);
            //topics
            hasmetacomponents |= false;
            if (topics == null)
                topics = new string[0];
            pieces.Add(BitConverter.GetBytes(topics.Length));
            for (int i=0;i<topics.Length; i++) {
                //topics[i]
                if (topics[i] == null)
                    topics[i] = "";
                scratch1 = Encoding.ASCII.GetBytes((string)topics[i]);
                thischunk = new byte[scratch1.Length + 4];
                scratch2 = BitConverter.GetBytes(scratch1.Length);
                Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
                Array.Copy(scratch2, thischunk, 4);
                pieces.Add(thischunk);
            }
            // combine every array in pieces into one array and return it
            int __a_b__f = pieces.Sum((__a_b__c)=>__a_b__c.Length);
            int __a_b__e=0;
            byte[] __a_b__d = new byte[__a_b__f];
            foreach(var __p__ in pieces)
            {
                Array.Copy(__p__,0,__a_b__d,__a_b__e,__p__.Length);
                __a_b__e += __p__.Length;
            }
            return __a_b__d;
        }

        public override void Randomize()
        {
            int arraylength = -1;
            Random rand = new Random();
            int strlength;
            byte[] strbuf, myByte;

            //header
            header = new Header();
            header.Randomize();
            //level
            myByte = new byte[1];
            rand.NextBytes(myByte);
            level= myByte[0];
            //name
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            name = Encoding.ASCII.GetString(strbuf);
            //msg
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            msg = Encoding.ASCII.GetString(strbuf);
            //file
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            file = Encoding.ASCII.GetString(strbuf);
            //function
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            function = Encoding.ASCII.GetString(strbuf);
            //line
            line = (uint)rand.Next();
            //topics
            arraylength = rand.Next(10);
            if (topics == null)
                topics = new string[arraylength];
            else
                Array.Resize(ref topics, arraylength);
            for (int i=0;i<topics.Length; i++) {
                //topics[i]
                strlength = rand.Next(100) + 1;
                strbuf = new byte[strlength];
                rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
                for (int __x__ = 0; __x__ < strlength; __x__++)
                    if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                        strbuf[__x__] = (byte)(rand.Next(254) + 1);
                strbuf[strlength - 1] = 0; //null terminate
                topics[i] = Encoding.ASCII.GetString(strbuf);
            }
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            rosgraph_msgs.Log other = (Messages.rosgraph_msgs.Log)____other;

            ret &= header.Equals(other.header);
            ret &= level == other.level;
            ret &= name == other.name;
            ret &= msg == other.msg;
            ret &= file == other.file;
            ret &= function == other.function;
            ret &= line == other.line;
            if (topics.Length != other.topics.Length)
                return false;
            for (int __i__=0; __i__ < topics.Length; __i__++)
            {
                ret &= topics[__i__] == other.topics[__i__];
            }
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
