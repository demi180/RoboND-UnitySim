using System;
using System.Collections.Generic;
using System.Text;


namespace Uml.Robotics.Ros.Transforms
{
    public class Vector3
    {
        public double x;
        public double y;
        public double z;

        public Vector3()
            : this(0, 0, 0)
        {
        }

        public Vector3(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public Vector3(Vector3 shallow)
            : this(shallow.x, shallow.y, shallow.z)
        {
        }

        public Vector3(Messages.geometry_msgs.Vector3 shallow)
            : this(shallow.x, shallow.y, shallow.z)
        {
        }

        public Messages.geometry_msgs.Vector3 ToMsg()
        {
            return new Messages.geometry_msgs.Vector3 { x = x, y = y, z = z };
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3 operator -(Vector3 v1)
        {
            return new Vector3(-v1.x, -v1.y, -v1.z);
        }

        public static Vector3 operator *(Vector3 v1, float d)
        {
            return v1 * ((double)d);
        }

        public static Vector3 operator *(Vector3 v1, int d)
        {
            return v1 * ((double)d);
        }

        public static Vector3 operator *(Vector3 v1, double d)
        {
            return new Vector3(d * v1.x, d * v1.y, d * v1.z);
        }

        public static Vector3 operator *(float d, Vector3 v1)
        {
            return v1 * ((double)d);
        }

        public static Vector3 operator *(int d, Vector3 v1)
        {
            return v1 * ((double)d);
        }

        public static Vector3 operator *(double d, Vector3 v1)
        {
            return v1 * d;
        }

        public double dot(Vector3 v2)
        {
            return x * v2.x + y * v2.y + z * v2.z;
        }

        public override string ToString()
        {
            return string.Format("({0:F4},{1:F4},{2:F4})", x, y, z);
        }

        public void setInterpolate3(Vector3 v0, Vector3 v1, double rt)
        {
            double s = 1.0 - rt;
            x = s * v0.x + rt * v1.x;
            y = s * v0.y + rt * v1.y;
            z = s * v0.z + rt * v1.z;
        }
    }
}
