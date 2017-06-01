using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uml.Robotics.Ros;


namespace Messages.geometry_msgs
{
    public class Vector3 : RosMessage
    {
        public double x;
        public double y;
        public double z;

        public override string MD5Sum() { return "4a842b65f413084dc2b10fb484ea7f17"; }
        public override bool HasHeader() { return false; }
        public override bool IsMetaType() { return false; }
        public override string MessageDefinition() { return @"float64 x
float64 y
float64 z"; }
        public override string MessageType { get { return "geometry_msgs/Vector3"; } }
        public override bool IsServiceComponent() { return false; }

        public Vector3()
        {
        }

        public Vector3(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public Vector3(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            int piecesize = 0;
            IntPtr h;

            //x
            piecesize = Marshal.SizeOf(typeof(double));
            h = IntPtr.Zero;
            if (serializedMessage.Length - currentIndex != 0)
            {
                h = Marshal.AllocHGlobal(piecesize);
                Marshal.Copy(serializedMessage, currentIndex, h, piecesize);
            }
            if (h == IntPtr.Zero) throw new Exception("Memory allocation failed");
            x = (double)Marshal.PtrToStructure(h, typeof(double));
            Marshal.FreeHGlobal(h);
            currentIndex+= piecesize;
            //y
            piecesize = Marshal.SizeOf(typeof(double));
            h = IntPtr.Zero;
            if (serializedMessage.Length - currentIndex != 0)
            {
                h = Marshal.AllocHGlobal(piecesize);
                Marshal.Copy(serializedMessage, currentIndex, h, piecesize);
            }
            if (h == IntPtr.Zero) throw new Exception("Memory allocation failed");
            y = (double)Marshal.PtrToStructure(h, typeof(double));
            Marshal.FreeHGlobal(h);
            currentIndex+= piecesize;
            //z
            piecesize = Marshal.SizeOf(typeof(double));
            h = IntPtr.Zero;
            if (serializedMessage.Length - currentIndex != 0)
            {
                h = Marshal.AllocHGlobal(piecesize);
                Marshal.Copy(serializedMessage, currentIndex, h, piecesize);
            }
            if (h == IntPtr.Zero) throw new Exception("Memory allocation failed");
            z = (double)Marshal.PtrToStructure(h, typeof(double));
            Marshal.FreeHGlobal(h);
            currentIndex+= piecesize;
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            byte[] scratch1;
            List<byte[]> pieces = new List<byte[]>();
            GCHandle h;

            //x
            scratch1 = new byte[Marshal.SizeOf(typeof(double))];
            h = GCHandle.Alloc(scratch1, GCHandleType.Pinned);
            Marshal.StructureToPtr(x, h.AddrOfPinnedObject(), false);
            h.Free();
            pieces.Add(scratch1);
            //y
            scratch1 = new byte[Marshal.SizeOf(typeof(double))];
            h = GCHandle.Alloc(scratch1, GCHandleType.Pinned);
            Marshal.StructureToPtr(y, h.AddrOfPinnedObject(), false);
            h.Free();
            pieces.Add(scratch1);
            //z
            scratch1 = new byte[Marshal.SizeOf(typeof(double))];
            h = GCHandle.Alloc(scratch1, GCHandleType.Pinned);
            Marshal.StructureToPtr(z, h.AddrOfPinnedObject(), false);
            h.Free();
            pieces.Add(scratch1);
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

            //x
            x = (rand.Next() + rand.NextDouble());
            //y
            y = (rand.Next() + rand.NextDouble());
            //z
            z = (rand.Next() + rand.NextDouble());
        }

        public override bool Equals(RosMessage ____other)
        {
            if (____other == null)
                return false;
            bool ret = true;
            geometry_msgs.Vector3 other = (Messages.geometry_msgs.Vector3)____other;

            ret &= x == other.x;
            ret &= y == other.y;
            ret &= z == other.z;
            // for each SingleType st:
            //    ret &= {st.Name} == other.{st.Name};
            return ret;
        }
    }
}
