using Messages;

namespace Uml.Robotics.Ros
{
    internal class MessageAndSerializerFunc
    {
        internal RosMessage msg;
        internal bool nocopy;
        internal TopicManager.SerializeFunc serfunc;
        internal bool serialize;

        internal MessageAndSerializerFunc(RosMessage msg, TopicManager.SerializeFunc serfunc, bool ser, bool nc)
        {
            this.msg = msg;
            this.serfunc = serfunc;
            serialize = ser;
            nocopy = nc;
        }
    }
}
