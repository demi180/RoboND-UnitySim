using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uml.Robotics.Ros;
using std_msgs = Messages.std_msgs;

namespace Uml.Robotics.Ros.Transforms
{
    public enum TF_STATUS
    {
        NO_ERROR,
        LOOKUP_ERROR,
        CONNECTIVITY_ERROR,
        EXTRAPOLATION_ERROR
    }

    public enum WalkEnding
    {
        Identity,
        TargetParentOfSource,
        SourceParentOfTarget,
        FullPath
    }

    public class Stamped<T>
    {
        public T data;
        public string frame_id;
        public std_msgs.Time stamp;

        public Stamped()
        {
        }

        public Stamped(std_msgs.Time t, string f, T d)
        {
            stamp = t;
            frame_id = f;
            data = d;
        }
    }

    public struct TimeAndFrameID
    {
        public uint frame_id;
        public ulong time;

        public TimeAndFrameID(ulong t, uint f)
        {
            time = t;
            frame_id = f;
        }
    }

    public class TransformStorage
    {
        public uint child_frame_id;
        public uint frame_id;
        public Quaternion rotation;
        public ulong stamp;
        public Vector3 translation;

        public TransformStorage()
        {
            this.rotation = new Quaternion();
            this.translation = new Vector3();
        }

        public TransformStorage(Transform data, uint frameId, uint childFrameId)
        {
            this.rotation = data.basis;
            this.translation = data.origin;
            this.stamp = TimeCache.toLong(data.stamp.data);
            this.frame_id = frameId;
            this.child_frame_id = childFrameId;
        }
    }
}
