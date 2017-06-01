using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;

namespace Messages.tf
{
    public class tfMessage : RosMessage
    {
        public Messages.geometry_msgs.TransformStamped[] transforms;

        public override string MD5Sum() { return "94810edda583a504dfda3829e70d7eec"; }
        public override bool HasHeader() { return false; }
        public override bool IsMetaType() { return true; }
        public override string MessageDefinition() { return @"geometry_msgs/TransformStamped[] transforms"; }
        public override string MessageType { get { return "tf/tfMessage"; } }
        public override bool IsServiceComponent() { return false; }

        public tfMessage()
        {
        }

        public tfMessage(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public tfMessage(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            int arraylength = -1;
            bool hasmetacomponents = false;

            //transforms
            hasmetacomponents |= true;
            arraylength = BitConverter.ToInt32(serializedMessage, currentIndex);
            currentIndex += Marshal.SizeOf(typeof(System.Int32));
            if (transforms == null)
                transforms = new Messages.geometry_msgs.TransformStamped[arraylength];
            else
                Array.Resize(ref transforms, arraylength);
            for (int i=0;i<transforms.Length; i++) {
                //transforms[i]
                transforms[i] = new Messages.geometry_msgs.TransformStamped(serializedMessage, ref currentIndex);
            }
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            bool hasmetacomponents = false;
            List<byte[]> pieces = new List<byte[]>();

            //transforms
            hasmetacomponents |= true;
            if (transforms == null)
                transforms = new Messages.geometry_msgs.TransformStamped[0];
            pieces.Add(BitConverter.GetBytes(transforms.Length));
            for (int i=0;i<transforms.Length; i++) {
                //transforms[i]
                if (transforms[i] == null)
                    transforms[i] = new Messages.geometry_msgs.TransformStamped();
                pieces.Add(transforms[i].Serialize(true));
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

            //transforms
            arraylength = rand.Next(10);
            if (transforms == null)
                transforms = new Messages.geometry_msgs.TransformStamped[arraylength];
            else
                Array.Resize(ref transforms, arraylength);
            for (int i=0;i<transforms.Length; i++) {
                //transforms[i]
                transforms[i] = new Messages.geometry_msgs.TransformStamped();
                transforms[i].Randomize();
            }
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            tf.tfMessage other = (Messages.tf.tfMessage)____other;

            if (transforms.Length != other.transforms.Length)
                return false;
            for (int __i__=0; __i__ < transforms.Length; __i__++)
            {
                ret &= transforms[__i__].Equals(other.transforms[__i__]);
            }
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
