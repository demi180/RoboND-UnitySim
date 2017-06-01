using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;


namespace Messages.geometry_msgs
{
    public class Transform : RosMessage
    {
		public Messages.geometry_msgs.Vector3 translation = new Messages.geometry_msgs.Vector3();
		public Messages.geometry_msgs.Quaternion rotation = new Messages.geometry_msgs.Quaternion();

        public override string MD5Sum() { return "ac9eff44abf714214112b05d54a3cf9b"; }
        public override bool HasHeader() { return false; }
        public override bool IsMetaType() { return true; }
        public override string MessageDefinition() { return @"Vector3 translation
Quaternion rotation"; }
        public override string MessageType { get { return "geometry_msgs/Transform"; } }
        public override bool IsServiceComponent() { return false; }

        public Transform()
        {
        }

        public Transform(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public Transform(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            //translation
            translation = new Messages.geometry_msgs.Vector3(serializedMessage, ref currentIndex);
            //rotation
            rotation = new Messages.geometry_msgs.Quaternion(serializedMessage, ref currentIndex);
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            List<byte[]> pieces = new List<byte[]>();

            //translation
            if (translation == null)
                translation = new Messages.geometry_msgs.Vector3();
            pieces.Add(translation.Serialize(true));
            //rotation
            if (rotation == null)
                rotation = new Messages.geometry_msgs.Quaternion();
            pieces.Add(rotation.Serialize(true));
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

            //translation
            translation = new Messages.geometry_msgs.Vector3();
            translation.Randomize();
            //rotation
            rotation = new Messages.geometry_msgs.Quaternion();
            rotation.Randomize();
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
				return false;
            bool ret = true;
            geometry_msgs.Transform other = (Messages.geometry_msgs.Transform)____other;

            ret &= translation.Equals(other.translation);
            ret &= rotation.Equals(other.rotation);
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
