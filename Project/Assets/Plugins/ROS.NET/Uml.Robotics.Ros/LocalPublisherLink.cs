using System;
using System.Collections.Generic;

namespace Uml.Robotics.Ros
{
    public class LocalPublisherLink : PublisherLink
    {
        private object gate = new object();
        private bool dropped;
        private LocalSubscriberLink publisher = null;

        public LocalPublisherLink(Subscription parent, string xmlrpc_uri)
            : base(parent, xmlrpc_uri)
        {
        }

        public override string TransportType
        {
            get { return "INTRAPROCESS"; }
        }

        public void setPublisher(LocalSubscriberLink pub_link)
        {
            lock (parent)
            {
                var header = new Dictionary<string, string>();
                header["topic"] = parent.name;
                header["md5sum"] = parent.md5sum;
                header["callerid"] = ThisNode.Name;
                header["type"] = parent.datatype;
                header["tcp_nodelay"] = "1";
                setHeader(new Header { Values = header });
            }
        }

        public override void drop()
        {
            lock (gate)
            {
                if (dropped)
                    return;
                dropped = true;
            }

            if (publisher != null)
            {
                publisher.Drop();
            }

            lock (parent)
            {
                parent.removePublisherLink(this);
            }
        }

        public void handleMessage<T>(T m, bool ser, bool nocopy) where T : RosMessage, new()
        {
            stats.messagesReceived++;
            if (m.Serialized == null)
            {
                // ignore stats to avoid an unnecessary allocation
            }
            else
            {
                stats.bytesReceived += m.Serialized.Length;
            }
            if (parent != null)
            {
                lock (parent)
                {
                    stats.drops += parent.handleMessage(m, ser, nocopy, m.connection_header, this);
                }
            }
        }

        public void getPublishTypes(ref bool ser, ref bool nocopy, string messageType)
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
            if (parent != null)
            {
                lock (parent)
                {
                    parent.getPublishTypes(ref ser, ref nocopy, messageType);
                }
            }
            else
            {
                ser = true;
                nocopy = false;
            }
        }
    }
}