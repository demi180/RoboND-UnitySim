using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uml.Robotics.Ros;

namespace Uml.Robotics.Ros.Transforms
{
    public abstract class ATransformAccum
    {
        public TransformStorage st;
        public abstract uint gather(TimeCache cache, ulong time, out string error_str);
        public abstract void accum(bool source);
        public abstract void finalize(WalkEnding end, ulong time);
    }

    public class CanTransformAccum : ATransformAccum
    {
        public override uint gather(TimeCache cache, ulong time, out string error_str)
        {
            return cache.getParent(time, out error_str);
        }

        public override void accum(bool source)
        {
        }

        public override void finalize(WalkEnding end, ulong time)
        {
        }
    }

    public class TransformAccum : ATransformAccum
    {
        public Quaternion result_quat;
        public Vector3 result_vec;
        public Quaternion source_to_top_quat = new Quaternion();
        public Vector3 source_to_top_vec = new Vector3();
        public Quaternion target_to_top_quat = new Quaternion();
        public Vector3 target_to_top_vec = new Vector3();
        public ulong time;

        public override uint gather(TimeCache cache, ulong time_, out string error_str)
        {
            if (!cache.getData(time_, ref st, out error_str))
                return 0;
            return st.frame_id;
        }

        public override void finalize(WalkEnding end, ulong _time)
        {
            switch (end)
            {
                case WalkEnding.Identity:
                    break;
                case WalkEnding.TargetParentOfSource:
                    result_vec = source_to_top_vec;
                    result_quat = source_to_top_quat;
                    break;
                case WalkEnding.SourceParentOfTarget:
                    {
                        Quaternion inv_target_quat = target_to_top_quat.inverse();
                        Vector3 inv_target_vec = quatRotate(inv_target_quat, -1 * target_to_top_vec);
                        result_quat = inv_target_quat;
                        result_vec = inv_target_vec;
                    }
                    break;
                case WalkEnding.FullPath:
                    {
                        Quaternion inv_target_quat = target_to_top_quat.inverse();
                        Vector3 inv_target_vec = quatRotate(inv_target_quat, -1 * target_to_top_vec);
                        result_vec = quatRotate(inv_target_quat, source_to_top_vec) + inv_target_vec;
                        result_quat = inv_target_quat * source_to_top_quat;
                    }
                    break;
            }
            time = _time;
        }

        public override void accum(bool source)
        {
            if (source)
            {
                source_to_top_vec = quatRotate(st.rotation, source_to_top_vec) + st.translation;
                source_to_top_quat = st.rotation * source_to_top_quat;
            }
            else
            {
                target_to_top_vec = quatRotate(st.rotation, target_to_top_vec) + st.translation;
                target_to_top_quat = st.rotation * target_to_top_quat;
            }
        }

        public Vector3 quatRotate(Quaternion rotation, Vector3 v)
        {
            Quaternion q = rotation * v;
            q *= rotation.inverse();
            return new Vector3(q.x, q.y, q.z);
        }
    }
}
