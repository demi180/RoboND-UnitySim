namespace Uml.Robotics.Ros
{
    public delegate void SubscriberStatusCallback(SingleSubscriberPublisher pub);

    public class SubscriberCallbacks
    {
        public ICallbackQueue CallbackQueue;
        public SubscriberStatusCallback connect;
        public SubscriberStatusCallback disconnect;
		public long CallbackId { get { return callbackID; } set { CallbackId = value; } }
		long callbackID = -1;

        public SubscriberCallbacks()
        {
        }

        public SubscriberCallbacks(
            SubscriberStatusCallback connectCB,
            SubscriberStatusCallback disconnectCB,
            ICallbackQueue callbackQueue
        )
        {
            this.connect = connectCB;
            this.disconnect = disconnectCB;
            this.CallbackQueue = callbackQueue;
        }
    }
}
