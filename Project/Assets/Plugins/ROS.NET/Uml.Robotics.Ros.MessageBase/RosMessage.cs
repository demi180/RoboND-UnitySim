using System;
using System.Collections.Generic;
using System.Reflection;

namespace Uml.Robotics.Ros
{
    public class RosMessage
    {
        public static RosMessage Generate(string rosMessageType)
        {
            var result = MessageTypeRegistry.Default.CreateMessage(rosMessageType);
            if (result == null)
            {
                throw new ArgumentException($"Could not find a RosMessage for {rosMessageType}.", nameof(rosMessageType));
            }

            return result;
        }

        public IDictionary<string, string> connection_header;
        private byte[] serialized;

        public virtual string MD5Sum() { return string.Empty; }
        public virtual bool HasHeader() { return false; }
        public virtual bool IsMetaType() { return false; }
        public virtual string MessageDefinition() { return string.Empty; }
        public virtual bool IsServiceComponent() { return false; }

        /// <summary>
        /// ROS message type
        /// </summary>
        public virtual string MessageType { get { return "xamla/unkown"; } }

        public byte[] Serialized { get => serialized; set => serialized = value; }

        public RosMessage()
        {
        }

        public RosMessage(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public RosMessage(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public void Deserialize(byte[] serializedMessage)
        {
            int start = 0;
            Deserialize(serializedMessage, ref start);
        }

        public virtual void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize()
        {
            return Serialize(false);
        }

        public virtual byte[] Serialize(bool isInnerMessage)
        {
            throw new NotImplementedException();
        }

        public virtual void Randomize()
        {
            throw new NotImplementedException();
        }

        public virtual bool Equals(RosMessage msg)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RosMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
