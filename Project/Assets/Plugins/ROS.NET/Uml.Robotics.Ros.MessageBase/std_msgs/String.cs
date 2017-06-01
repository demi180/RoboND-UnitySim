using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;


namespace Messages.std_msgs
{
    public class String : RosMessage
    {
        public string data = "";

        public override string MD5Sum() { return "992ce8a1687cec8c8bd883ec73ca41d1"; }
        public override bool HasHeader() { return false; }
        public override bool IsMetaType() { return false; }
        public override string MessageDefinition() { return @"string data"; }
        public override string MessageType { get { return "std_msgs/String"; } }
        public override bool IsServiceComponent() { return false; }

        public String()
        {
        }

        public String(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public String(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public String(string d)
        {
            data = d;
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            int piecesize = 0;

            //data
            data = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            data = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            byte[] thischunk, scratch1, scratch2;
            List<byte[]> pieces = new List<byte[]>();

            //data
            if (data == null)
                data = "";
            scratch1 = Encoding.ASCII.GetBytes((string)data);
            thischunk = new byte[scratch1.Length + 4];
            scratch2 = BitConverter.GetBytes(scratch1.Length);
            Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
            Array.Copy(scratch2, thischunk, 4);
            pieces.Add(thischunk);
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
            Random rand = new Random();
            int strlength;
            byte[] strbuf;

            //data
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            data = Encoding.ASCII.GetString(strbuf);
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            std_msgs.String other = (Messages.std_msgs.String)____other;

            ret &= data == other.data;
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
