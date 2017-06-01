using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;
using Messages.std_msgs;

namespace Messages.geometry_msgs
{
    public class TransformStamped : RosMessage
    {
        public Header header = new Header();
        public string child_frame_id = "";
        public Messages.geometry_msgs.Transform transform = new Messages.geometry_msgs.Transform();

        public override string MD5Sum() { return "b5764a33bfeb3588febc2682852579b0"; }
        public override bool HasHeader() { return true; }
        public override bool IsMetaType() { return true; }
        public override string MessageDefinition() { return @"Header header
string child_frame_id
Transform transform"; }
        public override string MessageType { get { return "geometry_msgs/TransformStamped"; } }
        public override bool IsServiceComponent() { return false; }

        public TransformStamped()
        {
        }

        public TransformStamped(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public TransformStamped(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            int piecesize = 0;

            //header
            header = new Header(serializedMessage, ref currentIndex);
            //child_frame_id
            child_frame_id = "";
            piecesize = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += 4;
            child_frame_id = Encoding.ASCII.GetString(serializedMessage, currentIndex, piecesize);
            currentIndex += piecesize;
            //transform
            transform = new Messages.geometry_msgs.Transform(serializedMessage, ref currentIndex);
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            byte[] thischunk, scratch1, scratch2;
            List<byte[]> pieces = new List<byte[]>();

            //header
            if (header == null)
                header = new Header();
            pieces.Add(header.Serialize(true));
            //child_frame_id
            if (child_frame_id == null)
                child_frame_id = "";
            scratch1 = Encoding.ASCII.GetBytes((string)child_frame_id);
            thischunk = new byte[scratch1.Length + 4];
            scratch2 = BitConverter.GetBytes(scratch1.Length);
            Array.Copy(scratch1, 0, thischunk, 4, scratch1.Length);
            Array.Copy(scratch2, thischunk, 4);
            pieces.Add(thischunk);
            //transform
            if (transform == null)
                transform = new Messages.geometry_msgs.Transform();
            pieces.Add(transform.Serialize(true));
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

            //header
            header = new Header();
            header.Randomize();
            //child_frame_id
            strlength = rand.Next(100) + 1;
            strbuf = new byte[strlength];
            rand.NextBytes(strbuf);  //fill the whole buffer with random bytes
            for (int __x__ = 0; __x__ < strlength; __x__++)
                if (strbuf[__x__] == 0) //replace null chars with non-null random ones
                    strbuf[__x__] = (byte)(rand.Next(254) + 1);
            strbuf[strlength - 1] = 0; //null terminate
            child_frame_id = Encoding.ASCII.GetString(strbuf);
            //transform
            transform = new Messages.geometry_msgs.Transform();
            transform.Randomize();
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            geometry_msgs.TransformStamped other = (Messages.geometry_msgs.TransformStamped)____other;

            ret &= header.Equals(other.header);
            ret &= child_frame_id == other.child_frame_id;
            ret &= transform.Equals(other.transform);
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
