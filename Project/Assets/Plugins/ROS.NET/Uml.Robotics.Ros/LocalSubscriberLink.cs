using System;

namespace Uml.Robotics.Ros
{
    public class LocalSubscriberLink : SubscriberLink
    {
        private object gate = new object();
        private bool dropped;
        private LocalPublisherLink subscriber;

        public LocalSubscriberLink(Publication pub)
        {
            parent = pub;
            topic = parent.Name;
        }

        public void SetSubscriber(LocalPublisherLink publisherLink)
        {
            subscriber = publisherLink;
            connection_id = ConnectionManager.Instance.GetNewConnectionId();
            destination_caller_id = ThisNode.Name;
        }

        internal override void EnqueueMessage(MessageAndSerializerFunc holder)
        {
            lock (gate)
            {
                if (dropped)
                    return;
            }

            if (subscriber != null)
                subscriber.handleMessage(holder.msg, holder.serialize, holder.nocopy);
        }

        public override void Drop()
        {
            lock (gate)
            {
                if (dropped)
                    return;
                dropped = true;
            }

            if (subscriber != null)
            {
                subscriber.drop();
            }

            lock (parent)
            {
                parent.removeSubscriberLink(this);
            }
        }

        public override void getPublishTypes(ref bool ser, ref bool nocopy, string messageType)
        {
            lock (gate)
            {
                if (dropped)
                {
                    ser = false;
                    nocopy = false;
                    return;
                }
            }

            subscriber.getPublishTypes(ref ser, ref nocopy, messageType);
        }
    }
}
