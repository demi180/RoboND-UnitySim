namespace Uml.Robotics.Ros
{
    public class SubscribeOptions
    {
        public bool allow_concurrent_callbacks = true;
        public ICallbackQueue callback_queue;
        public string datatype = "";
        public bool has_header;
        public ISubscriptionCallbackHelper helper;
        public bool latch;
        public string md5sum = "";
        public string message_definition = "";
        public int queue_size;
        public string topic = "";

        public SubscribeOptions(string topic, string dataType, string md5sum, int queueSize, ISubscriptionCallbackHelper callbackHelper)
        {
            this.topic = topic;
            this.queue_size = queueSize;
            this.helper = callbackHelper;
            this.datatype = dataType;
            this.md5sum = md5sum;
        }
    }

    public delegate void CallbackDelegate<in T>(T argument) where T : RosMessage, new();

    public class SubscribeOptions<T>
        : SubscribeOptions
        where T : RosMessage, new()
    {
        public SubscribeOptions()
             : this("", 1)
        {
        }

        public SubscribeOptions(string topic, int queueSize, CallbackDelegate<T> callbackHelper = null)
            : base(topic, null, null, queueSize, null)
        {
            var generic = new T();
            if (callbackHelper != null)
                helper = new SubscriptionCallbackHelper<T>(generic.MessageType, callbackHelper);
            else
                helper = new SubscriptionCallbackHelper<T>(generic.MessageType);


            datatype = generic.MessageType;
            md5sum = generic.MD5Sum();
        }
    }
}
