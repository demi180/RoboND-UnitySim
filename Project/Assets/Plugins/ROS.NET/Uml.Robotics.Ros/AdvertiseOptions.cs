using System;
using System.Collections.Generic;
using Messages;

namespace Uml.Robotics.Ros
{
    public class AdvertiseOptions<T> where T : RosMessage, new()
    {
        public readonly SubscriberStatusCallback connectCB;
        public readonly SubscriberStatusCallback disconnectCB;
        public readonly string dataType;
        public readonly bool hasHeader;
        public readonly string md5Sum;
        public readonly string messageDefinition;
        public readonly int queueSize;

        public string topic;
        public bool latch;
        public ICallbackQueue callbackQueue;

        public AdvertiseOptions(string topic, int queueSize)
            : this(topic, queueSize, null, null)
        {
        }

        public AdvertiseOptions(
            string topic,
            int queueSize,
            SubscriberStatusCallback connectCallback,
            SubscriberStatusCallback disconnectCallback,
            ICallbackQueue callbackQueue = null
        )
        : this(
            topic,
            queueSize,
            new T().MD5Sum(),
            new T().MessageType,
            new T().MessageDefinition(),
            connectCallback,
            disconnectCallback
        )
        {
        }

        public AdvertiseOptions(
            string topic,
            int queueSize,
            string md5Sum,
            string dataType,
            string messageDefinition,
            SubscriberStatusCallback connectcallback = null,
            SubscriberStatusCallback disconnectcallback = null,
            ICallbackQueue callbackQueue = null
        )
        {
            this.topic = topic;
            this.md5Sum = md5Sum;
            this.queueSize = queueSize;
            this.callbackQueue = callbackQueue;

            T tt = new T();
            this.dataType = dataType.Length > 0 ? dataType : tt.MessageType;
            this.messageDefinition = string.IsNullOrEmpty(messageDefinition) ? tt.MessageDefinition() : messageDefinition;
            this.hasHeader = tt.HasHeader();

            this.connectCB = connectcallback;
            this.disconnectCB = disconnectcallback;
        }

        public static AdvertiseOptions<M> Create<M>(
            string topic,
            int queueSize,
            SubscriberStatusCallback connectcallback,
            SubscriberStatusCallback disconnectcallback,
            ICallbackQueue callbackQueue
        )
            where M : RosMessage, new()
        {
            return new AdvertiseOptions<M>(topic, queueSize, connectcallback, disconnectcallback, callbackQueue);
        }
    }
}