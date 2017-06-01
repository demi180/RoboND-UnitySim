using System;
using System.Collections.Generic;
using System.Text;

namespace Uml.Robotics.Ros.Transforms
{
    public class Quaternion
    {
        public double w, x, y, z;

        public Quaternion()
            : this(0, 0, 0, 1)
        {
        }

        public Quaternion(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(Quaternion shallow)
            : this(shallow.x, shallow.y, shallow.z, shallow.w)
        {
        }

        public Quaternion(Messages.geometry_msgs.Quaternion shallow)
            : this(shallow.x, shallow.y, shallow.z, shallow.w)
        {
        }

        public Messages.geometry_msgs.Quaternion ToMsg()
        {
            return new Messages.geometry_msgs.Quaternion { w = w, x = x, y = y, z = z };
        }

        public static Quaternion operator +(Quaternion v1, Quaternion v2)
        {
            return new Quaternion(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }

        public static Quaternion operator -(Quaternion v1)
        {
            return new Quaternion(-v1.x, -v1.y, -v1.z, -v1.w);
        }

        public static Quaternion operator -(Quaternion v1, Quaternion v2)
        {
            return v1 + (-v2);
        }

        public static Quaternion operator *(Quaternion v1, float d)
        {
            return v1 * (double)d;
        }

        public static Quaternion operator *(Quaternion v1, int d)
        {
            return v1 * (double)d;
        }

        public static Quaternion operator *(Quaternion v1, double d)
        {
            return new Quaternion(v1.x * d, v1.y * d, v1.z * d, v1.w * d);
        }

        public static Quaternion operator *(float d, Quaternion v1)
        {
            return v1 * (double)d;
        }

        public static Quaternion operator *(int d, Quaternion v1)
        {
            return v1 * (double)d;
        }

        public static Quaternion operator *(double d, Quaternion v1)
        {
            return v1 * d;
        }

        public static Quaternion operator *(Quaternion v1, Quaternion v2)
        {
            return new Quaternion(v1.x * v2.w + v1.y * v2.z - v1.z * v2.y + v1.w * v2.x,
                                    -v1.x * v2.z + v1.y * v2.w + v1.z * v2.x + v1.w * v2.y,
                                    v1.x * v2.y - v1.y * v2.x + v1.z * v2.w + v1.w * v2.z,
                                    -v1.x * v2.x - v1.y * v2.y - v1.z * v2.z + v1.w * v2.w);
        }

        public static Quaternion operator *(Quaternion v1, Vector3 v2)
        {
            return v1 * new Quaternion(v2.x, v2.y, v2.z, 0.0) * v1.inverse();
        }

        public static Quaternion operator /(Quaternion v1, float s)
        {
            return v1 / (double)s;
        }

        public static Quaternion operator /(Quaternion v1, int s)
        {
            return v1 / (double)s;
        }

        public static Quaternion operator /(Quaternion v1, double s)
        {
            return v1 * (1.0 / s);
        }

        public Quaternion inverse()
        {
            return new Quaternion(-x / norm, -y / norm, -z / norm, w / norm);
        }

        public double dot(Quaternion q)
        {
            return x * q.x + y * q.y + z * q.z + w * q.w;
        }

        public double length2()
        {
            return abs * abs;
        }

        public double length()
        {
            return abs;
        }

        public double norm
        {
            get { return (x * x) + (y * y) + (z * z) + (w * w); }
        }

        public double abs
        {
            get { return Math.Sqrt(norm); }
        }

        public double angle
        {
            get { return Math.Acos(w / abs) * 2.0; }
        }

        public override string ToString()
        {
            return string.Format("quat=({0:F4},{1:F4},{2:F4},{3:F4})" /*, rpy={4}"*/, w, x, y, z /*, getRPY()*/);
        }

        public Vector3 getRPY()
        {
            Vector3 tmp = new Matrix3x3(this).getYPR();
            return new Vector3(tmp.z, tmp.y, tmp.x);
        }

        public static Quaternion FromRPY(Vector3 rpy)
        {
            double halfroll = rpy.x / 2;
            double halfpitch = rpy.y / 2;
            double halfyaw = rpy.z / 2;

            double sin_r2 = Math.Sin(halfroll);
            double sin_p2 = Math.Sin(halfpitch);
            double sin_y2 = Math.Sin(halfyaw);

            double cos_r2 = Math.Cos(halfroll);
            double cos_p2 = Math.Cos(halfpitch);
            double cos_y2 = Math.Cos(halfyaw);

            return new Quaternion(
                sin_r2 * cos_p2 * cos_y2 - cos_r2 * sin_p2 * sin_y2,
                cos_r2 * sin_p2 * cos_y2 + sin_r2 * cos_p2 * sin_y2,
                cos_r2 * cos_p2 * sin_y2 - sin_r2 * sin_p2 * cos_y2,
                cos_r2 * cos_p2 * cos_y2 + sin_r2 * sin_p2 * sin_y2
            );
        }

        public double angleShortestPath(Quaternion q)
        {
            double s = Math.Sqrt(length2() * q.length2());
            if (dot(q) < 0) // Take care of long angle case see http://en.wikipedia.org/wiki/Slerp
            {
                return Math.Acos(dot(-q) / s) * 2.0;
            }
            return Math.Acos(dot(q) / s) * 2.0;
        }

        public Quaternion slerp(Quaternion q, double t)
        {
            double theta = angleShortestPath(q);
            if (theta != 0)
            {
                double d = 1.0 / Math.Sin(theta);
                double s0 = Math.Sin((1.0 - t) * theta);
                double s1 = Math.Sin(t * theta);
                if (dot(q) < 0) // Take care of long angle case see http://en.wikipedia.org/wiki/Slerp
                {
                    return new Quaternion(
                        (x * s0 + -1 * q.x * s1) * d,
                        (y * s0 + -1 * q.y * s1) * d,
                        (z * s0 + -1 * q.z * s1) * d,
                        (w * s0 + -1 * q.w * s1) * d);
                }
                return new Quaternion(
                    (x * s0 + q.x * s1) * d,
                    (y * s0 + q.y * s1) * d,
                    (z * s0 + q.z * s1) * d,
                    (w * s0 + q.w * s1) * d);
            }
            return new Quaternion(this);
        }
    }
}
