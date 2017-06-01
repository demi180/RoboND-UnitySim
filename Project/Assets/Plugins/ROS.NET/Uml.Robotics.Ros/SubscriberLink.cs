using System;

namespace Uml.Robotics.Ros
{
    public class SubscriberLink
    {
        public class Stats
        {
            public long bytesSent;
            public long messageDataSent;
            public long messagesSent;
        }

        public uint connection_id;
        public string destination_caller_id = "";
        protected Publication parent;
        public Stats stats = new Stats();
        public string topic = "";

        public string Md5sum
        {
            get
            {
                lock (parent)
                {
                    return parent.Md5sum;
                }
            }
        }

        public string DataType
        {
            get
            {
                lock (parent)
                {
                    return parent.DataType;
                }
            }
        }

        public string MessageDefinition
        {
            get
            {
                lock (parent)
                {
                    return parent.MessageDefinition;
                }
            }
        }

        internal virtual void EnqueueMessage(MessageAndSerializerFunc holder)
        {
            throw new NotImplementedException();
        }

        public virtual void Drop()
        {
            throw new NotImplementedException();
        }

        public virtual void getPublishTypes(ref bool ser, ref bool nocopy, string type_info)
        {
            ser = true;
            nocopy = false;
        }
    }
}
