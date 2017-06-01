using System;

namespace Uml.Robotics.Ros
{
    public interface ICallbackQueue
    {
        void AddCallback(CallbackInterface callback);
        void CallAvailable(int timeout = ROS.WallDuration);

        void RemoveById(long ownerId);

        void Enable();
        void Disable();
        void Clear();
    }
}
