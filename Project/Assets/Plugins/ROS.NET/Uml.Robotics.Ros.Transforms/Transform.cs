using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Messages.std_msgs;

namespace Uml.Robotics.Ros.Transforms
{
    public class Transform
    {
        public string child_frame_id;
        public string frame_id;

        public Quaternion basis;
        public Time stamp;
        public Vector3 origin;

        public Transform()
            : this(new Quaternion(), new Vector3(), new Time(new TimeData()), "", "")
        {
        }

        public Transform(Messages.geometry_msgs.TransformStamped msg)
            : this(new Quaternion(msg.transform.rotation), new Vector3(msg.transform.translation), msg.header.stamp, msg.header.frame_id, msg.child_frame_id)
        {
        }

        public Transform(Quaternion q, Vector3 v, Time t = null, string fid = null, string cfi = null)
        {
            basis = q;
            origin = v;
            stamp = t;
            frame_id = fid;
            child_frame_id = cfi;
        }

        public static Transform operator *(Transform t, Transform v)
        {
            return new Transform(t.basis*v.basis, t*v.origin);
        }

        public static Vector3 operator *(Transform t, Vector3 v)
        {
            Matrix3x3 mat = new Matrix3x3(t.basis);
            return new Vector3(mat.m_el[0].dot(v) + t.origin.x,
                mat.m_el[1].dot(v) + t.origin.y,
                mat.m_el[2].dot(v) + t.origin.z);
        }

        public static Quaternion operator *(Transform t, Quaternion q)
        {
            return t.basis*q;
        }

        public override string ToString()
        {
            return "\ttranslation: " + origin + "\n\trotation: " + basis;
        }
    }
}