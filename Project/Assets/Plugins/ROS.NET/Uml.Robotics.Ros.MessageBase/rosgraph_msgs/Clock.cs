using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Uml.Robotics.Ros;
using Messages.std_msgs;


namespace Messages.rosgraph_msgs
{
    public class Clock : RosMessage
    {
        public Time clock = new Time();

        public override string MD5Sum() { return "a9c97c1d230cfc112e270351a944ee47"; }
        public override bool HasHeader() { return false; }
        public override bool IsMetaType() { return false; }
        public override string MessageDefinition() { return @"time clock"; }
        public override string MessageType { get { return "rosgraph_msgs/Clock"; } }
        public override bool IsServiceComponent() { return false; }

        public Clock()
        {
        }

        public Clock(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public Clock(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            //clock
            clock = new Time(new TimeData(
                    BitConverter.ToUInt32(serializedMessage, currentIndex),
                    BitConverter.ToUInt32(serializedMessage, currentIndex+Marshal.SizeOf(typeof(System.Int32)))));
            currentIndex += 2*Marshal.SizeOf(typeof(System.Int32));
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            List<byte[]> pieces = new List<byte[]>();
         
            //clock
            pieces.Add(BitConverter.GetBytes(clock.data.sec));
            pieces.Add(BitConverter.GetBytes(clock.data.nsec));
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

            //clock
            clock = new Time(new TimeData(
                    Convert.ToUInt32(rand.Next()),
                    Convert.ToUInt32(rand.Next())));
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            rosgraph_msgs.Clock other = (Messages.rosgraph_msgs.Clock)____other;

            ret &= clock.data.Equals(other.clock.data);
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
