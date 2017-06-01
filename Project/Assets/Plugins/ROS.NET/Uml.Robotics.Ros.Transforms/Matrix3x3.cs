using System;
using System.Collections.Generic;
using System.Text;

namespace Uml.Robotics.Ros.Transforms
{
    public class Matrix3x3
    {
        public Vector3[] m_el = new Vector3[3];

        public Matrix3x3()
            : this(0, 0, 0, 0, 0, 0, 0, 0, 0)
        {
        }

        public Matrix3x3(
            double xx, double xy, double xz,
            double yx, double yy, double yz,
            double zx, double zy, double zz)
        {
            setValue(xx, xy, xz, yx, yy, yz, zx, zy, zz);
        }

        public Matrix3x3(Quaternion q)
        {
            setRotation(q);
        }

        public void setValue(
            double xx, double xy, double xz,
            double yx, double yy, double yz,
            double zx, double zy, double zz)
        {
            m_el[0] = new Vector3(xx, xy, xz);
            m_el[1] = new Vector3(yx, yy, yz);
            m_el[2] = new Vector3(zx, zy, zz);
        }

        public void setRotation(Quaternion q)
        {
            double d = q.length2();
            double s = 2.0 / d;
            double xs = q.x * s, ys = q.y * s, zs = q.z * s;
            double wx = q.w * xs, wy = q.w * ys, wz = q.w * zs;
            double xx = q.x * xs, xy = q.x * ys, xz = q.x * zs;
            double yy = q.y * ys, yz = q.y * zs, zz = q.z * zs;
            setValue(1.0 - (yy + zz), xy - wz, xz + wy,
                xy + wz, 1.0 - (xx + zz), yz - wx,
                xz - wy, yz + wx, 1.0 - (xx + yy));
        }

        public static Vector3 operator *(Matrix3x3 mat1, Vector3 v1)
        {
            return new Vector3(mat1.m_el[0].x * v1.x + mat1.m_el[0].y * v1.y + mat1.m_el[0].z * v1.z, 
                               mat1.m_el[1].x * v1.x + mat1.m_el[1].y * v1.y + mat1.m_el[1].z * v1.z, 
                               mat1.m_el[2].x * v1.x + mat1.m_el[2].y * v1.y + mat1.m_el[2].z * v1.z);
        }

        internal Vector3 getYPR(uint solution_number = 1)
        {
            Euler euler_out;
            Euler euler_out2; //second solution

            // Check that pitch is not at a singularity
            if (Math.Abs(m_el[2].x) >= 1)
            {
                euler_out.yaw = 0;
                euler_out2.yaw = 0;

                // From difference of angles formula
                if (m_el[2].x < 0) //gimbal locked down
                {
                    double delta = Math.Atan2(m_el[0].y, m_el[0].z);
                    euler_out.pitch = Math.PI / 2.0d;
                    euler_out2.pitch = Math.PI / 2.0d;
                    euler_out.roll = delta;
                    euler_out2.roll = delta;
                }
                else // gimbal locked up
                {
                    double delta = Math.Atan2(-m_el[0].y, -m_el[0].z);
                    euler_out.pitch = -Math.PI / 2.0d;
                    euler_out2.pitch = -Math.PI / 2.0d;
                    euler_out.roll = delta;
                    euler_out2.roll = delta;
                }
            }
            else
            {
                euler_out.pitch = -Math.Asin(m_el[2].x);
                euler_out2.pitch = Math.PI - euler_out.pitch;

                euler_out.roll = Math.Atan2(m_el[2].y / Math.Cos(euler_out.pitch),
                    m_el[2].z / Math.Cos(euler_out.pitch));
                euler_out2.roll = Math.Atan2(m_el[2].y / Math.Cos(euler_out2.pitch),
                    m_el[2].z / Math.Cos(euler_out2.pitch));

                euler_out.yaw = Math.Atan2(m_el[1].x / Math.Cos(euler_out.pitch),
                    m_el[0].x / Math.Cos(euler_out.pitch));
                euler_out2.yaw = Math.Atan2(m_el[1].x / Math.Cos(euler_out2.pitch),
                    m_el[0].x / Math.Cos(euler_out2.pitch));
            }

            if (solution_number == 1)
            {
                return new Vector3(euler_out.yaw, euler_out.pitch, euler_out.roll);
            }
            return new Vector3(euler_out2.yaw, euler_out2.pitch, euler_out2.roll);
        }

        public override string ToString()
        {
            return string.Format("({0:F4},{1:F4},{2:F4}; {3:F4},{4:F4},{5:F4}; {6:F4},{7:F4},{8:F4})", 
                                 m_el[0].x, m_el[0].y, m_el[0].z, 
                                 m_el[1].x, m_el[1].y, m_el[1].z,
                                 m_el[2].x, m_el[2].y, m_el[2].z);
        }
        public struct Euler
        {
            public double pitch;
            public double roll;
            public double yaw;
        }
    }
}
