using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;
using Messages.std_msgs;

namespace Messages.actionlib_msgs
{
    public class GoalID : RosMessage
    {
        public Time stamp = new Time();
        public string id = "";

        public override string MD5Sum() { return "302881f31927c1df708a2dbab0e80ee8"; }
        public override bool HasHeader() { return false; }
        public override bool IsMetaType() { return false; }
        public override string MessageDefinition() { return @"time stamp
string id"; }
        public override string MessageType { get { return "actionlib_msgs/GoalID"; } }
        public override bool IsServiceComponent() { return false; }

        public GoalID()
        {
        }

        public GoalID(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public GoalID(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            int piecesize = 0;

            //stamp
            stamp = new Time(new TimeData(
                    BitConverter.ToUInt32(serializedMessage, currentIndex),
                    BitConverter.ToUInt32(serializedMessage, currentIndex+Marshal.SizeOf(typeof(System.Int32)))));
            currentIndex += 2*Marshal.SizeOf(typeof(System.Int32));
            //id
            id = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            id = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            byte[] thischunk, scratch1, scratch2;
            List<byte[]> pieces = new List<byte[]>();

            //stamp
            pieces.Add(BitConverter.GetBytes(stamp.data.sec));
            pieces.Add(BitConverter.GetBytes(stamp.data.nsec));
            //id
            if (id == null)
                id = "";
            scratch1 = Encoding.ASCII.GetBytes((string)id);
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

            //stamp
            stamp = new Time(new TimeData(
                    Convert.ToUInt32(rand.Next()),
                    Convert.ToUInt32(rand.Next())));
            //id
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            id = Encoding.ASCII.GetString(strbuf);
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            actionlib_msgs.GoalID other = (Messages.actionlib_msgs.GoalID)____other;

            ret &= stamp.data.Equals(other.stamp.data);
            ret &= id == other.id;
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
