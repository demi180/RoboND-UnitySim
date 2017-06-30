using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
//using Microsoft.Extensions.Logging;

using Uml.Robotics.XmlRpc;
using std_msgs = Messages.std_msgs;

namespace Uml.Robotics.Ros
{
    public class Publication
    {
        public bool Dropped;
        public Header connection_header;

        public readonly string DataType;
        public readonly bool HasHeader;
        public readonly bool Latch;
        public readonly int MaxQueue;
        public readonly string Md5sum;
        public readonly string MessageDefinition;
        public readonly string Name;

//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<Publication>();
        private uint _seq;
        private List<SubscriberCallbacks> callbacks = new List<SubscriberCallbacks>();
        private object callbacks_mutex = new object();
        internal MessageAndSerializerFunc last_message;
        internal Queue<MessageAndSerializerFunc> publish_queue = new Queue<MessageAndSerializerFunc>();
        private object publish_queue_mutex = new object();
        private object seq_mutex = new object();
        private List<SubscriberLink> subscriber_links = new List<SubscriberLink>();
        private object subscriber_links_mutex = new object();

        public Publication(string name, string datatype, string md5sum, string message_definition, int max_queue,
            bool latch, bool has_header)
        {
            Name = name;
            DataType = datatype;
            Md5sum = md5sum;
            MessageDefinition = message_definition;
            MaxQueue = max_queue;
            Latch = latch;
            HasHeader = has_header;
        }

        public int NumCallbacks
        {
            get { lock (callbacks_mutex) return callbacks.Count; }
        }

        public bool HasSubscribers
        {
            get { lock (subscriber_links_mutex) return subscriber_links.Count > 0; }
        }

        public int NumSubscribers
        {
            get { lock (subscriber_links_mutex) return subscriber_links.Count; }
        }

        public XmlRpcValue GetStats()
        {
            var stats = new XmlRpcValue();
            stats.Set(0, Name);
            var conn_data = new XmlRpcValue();
            conn_data.SetArray(0);
            lock (subscriber_links_mutex)
            {
                int cidx = 0;
                foreach (SubscriberLink sub_link in subscriber_links)
                {
                    var s = sub_link.stats;
                    var inside = new XmlRpcValue();
                    inside.Set(0, sub_link.connection_id);
                    inside.Set(1, s.bytesSent);
                    inside.Set(2, s.messageDataSent);
                    inside.Set(3, s.messagesSent);
                    inside.Set(4, 0);
                    conn_data.Set(cidx++, inside);
                }
            }
            stats.Set(1, conn_data);
            return stats;
        }

        public void drop()
        {
            lock (publish_queue_mutex)
            {
                lock (subscriber_links_mutex)
                {
                    if (Dropped)
                        return;
                    Dropped = true;
                }
            }
            dropAllConnections();
        }

        public void addSubscriberLink(SubscriberLink link)
        {
            lock (subscriber_links_mutex)
            {
                if (Dropped)
                    return;

                subscriber_links.Add(link);
                PollManager.Instance.AddPollThreadListener(processPublishQueue);
            }

            if (Latch && last_message != null)
            {
                link.EnqueueMessage(last_message);
            }

            peerConnect(link);
        }

        public void removeSubscriberLink(SubscriberLink link)
        {
            SubscriberLink lnk = null;
            lock (subscriber_links_mutex)
            {
                if (Dropped)
                    return;
                if (subscriber_links.Contains(link))
                {
                    lnk = link;
                    subscriber_links.Remove(lnk);
                    if (subscriber_links.Count == 0)
                        PollManager.Instance.RemovePollThreadListener(processPublishQueue);
                }
            }
            if (lnk != null)
                peerDisconnect(lnk);
        }

        internal void publish(MessageAndSerializerFunc msg)
        {
            lock (publish_queue_mutex)
            {
                publish_queue.Enqueue(msg);
            }
        }

        public bool validateHeader(Header header, ref string error_message)
        {
            string md5sum = "", topic = "", client_callerid = "";
            if (!header.Values.ContainsKey("md5sum") || !header.Values.ContainsKey("topic") ||
                !header.Values.ContainsKey("callerid"))
            {
                const string msg = "Header from subscriber did not have the required elements: md5sum, topic, callerid";
//                Logger.LogWarning(msg);
                error_message = msg;
                return false;
            }
            md5sum = (string) header.Values["md5sum"];
            topic = (string) header.Values["topic"];
            client_callerid = (string) header.Values["callerid"];
            if (Dropped)
            {
                string msg = "Received a tcpros connection for a nonexistent topic [" + topic + "] from [" +
                             client_callerid + "].";
//                Logger.LogWarning(msg);
                error_message = msg;
                return false;
            }

            if (Md5sum != md5sum && (md5sum != "*") && Md5sum != "*")
            {
                string datatype = header.Values.ContainsKey("type") ? (string) header.Values["type"] : "unknown";
                string msg = "Client [" + client_callerid + "] wants topic [" + topic + "] to hava datatype/md5sum [" +
                             datatype + "/" + md5sum + "], but our version has [" + DataType + "/" + Md5sum +
                             "]. Dropping connection";
//                Logger.LogWarning(msg);
                error_message = msg;
                return false;
            }
            return true;
        }

        public void getInfo(XmlRpcValue info)
        {
            lock (subscriber_links_mutex)
            {
                foreach (SubscriberLink c in subscriber_links)
                {
                    var curr_info = new XmlRpcValue();
                    curr_info.Set(0, (int) c.connection_id);
                    curr_info.Set(1, c.destination_caller_id);
                    curr_info.Set(2, "o");
                    curr_info.Set(3, "TCPROS");
                    curr_info.Set(4, Name);
                    info.Set(info.Count, curr_info);
                }
            }
        }

        public void addCallbacks(SubscriberCallbacks callbacks)
        {
            lock (callbacks_mutex)
            {
                this.callbacks.Add(callbacks);
                if (callbacks.connect != null && callbacks.CallbackQueue != null)
                {
                    lock (subscriber_links_mutex)
                    {
                        foreach (SubscriberLink i in subscriber_links)
                        {
                            CallbackInterface cb = new PeerConnDisconnCallback(callbacks.connect, i);
                            callbacks.CallbackId = cb.Uid;
                            callbacks.CallbackQueue.AddCallback(cb);
                        }
                    }
                }
            }
        }

        public void removeCallbacks(SubscriberCallbacks callbacks)
        {
            lock (callbacks_mutex)
            {
                if (callbacks.CallbackId >= 0)
                    callbacks.CallbackQueue.RemoveById(callbacks.CallbackId);
                if (this.callbacks.Contains(callbacks))
                    this.callbacks.Remove(callbacks);
            }
        }

        internal bool EnqueueMessage(MessageAndSerializerFunc holder)
        {
            lock (subscriber_links_mutex)
            {
                if (Dropped)
                    return false;
            }

            uint seq = incrementSequence();

            if (HasHeader)
            {
                object h = holder.msg.GetType().GetTypeInfo().GetField("header").GetValue(holder.msg);
                std_msgs.Header header;
                if (h == null)
                    header = new std_msgs.Header();
                else
                    header = (std_msgs.Header) h;
                header.seq = seq;
                if (header.stamp == null)
                {
                    header.stamp = ROS.GetTime();
                }
                if (header.frame_id == null)
                {
                    header.frame_id = "";
                }
                holder.msg.GetType().GetTypeInfo().GetField("header").SetValue(holder.msg, header);
            }
            holder.msg.connection_header = connection_header.Values;

            lock (subscriber_links_mutex)
            {
                foreach (SubscriberLink sub_link in subscriber_links)
                {
                    sub_link.EnqueueMessage(holder);
                }
            }

            if (Latch)
            {
                last_message = new MessageAndSerializerFunc(holder.msg, holder.serfunc, false, true);
            }
            return true;
        }

        public void dropAllConnections()
        {
            List<SubscriberLink> local_publishers = null;
            lock (subscriber_links_mutex)
            {
                local_publishers = new List<SubscriberLink>(subscriber_links);
                subscriber_links.Clear();
            }
            foreach (SubscriberLink link in local_publishers)
            {
                link.Drop();
            }
            local_publishers.Clear();
        }

        public void peerConnect(SubscriberLink sub_link)
        {
            //Logger.LogDebug($"PEER CONNECT: Id: {sub_link.connection_id} Dest: {sub_link.destination_caller_id} Topic: {sub_link.topic}");
            foreach (SubscriberCallbacks cbs in callbacks)
            {
                if (cbs.connect != null && cbs.CallbackQueue != null)
                {
                    var cb = new PeerConnDisconnCallback(cbs.connect, sub_link);
                    cbs.CallbackId = cb.Uid;
                    cbs.CallbackQueue.AddCallback(cb);
                }
            }
        }

        public void peerDisconnect(SubscriberLink sub_link)
        {
            //Logger.LogDebug("PEER DISCONNECT: [" + sub_link.topic + "]");
            foreach (SubscriberCallbacks cbs in callbacks)
            {
                if (cbs.disconnect != null && cbs.CallbackQueue != null)
                {
                    var cb = new PeerConnDisconnCallback(cbs.disconnect, sub_link);
                    cbs.CallbackId = cb.Uid;
                    cbs.CallbackQueue.AddCallback(cb);
                }
            }
        }

        public uint incrementSequence()
        {
            lock (seq_mutex)
            {
                return _seq++;
            }
        }

        public void processPublishQueue()
        {
            lock (publish_queue_mutex)
            {
                if (Dropped)
                    return;

                while (publish_queue.Count > 0)
                {
                    EnqueueMessage(publish_queue.Dequeue());
                }
            }
        }

        internal void getPublishTypes(ref bool serialize, ref bool nocopy, string messageType)
        {
            lock (subscriber_links_mutex)
            {
                foreach (SubscriberLink sub in subscriber_links)
                {
                    bool s = false, n = false;
                    sub.getPublishTypes(ref s, ref n, messageType);
                    serialize = serialize || s;
                    nocopy = nocopy || n;
                    if (serialize && nocopy)
                        break;
                }
            }
        }
    }

    public class PeerConnDisconnCallback : CallbackInterface
    {
//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<PeerConnDisconnCallback>();
        private SubscriberStatusCallback callback;
        private SubscriberLink sub_link;

        public PeerConnDisconnCallback(SubscriberStatusCallback callback, SubscriberLink sub_link)
        {
            this.callback = callback;
            this.sub_link = sub_link;
        }

        internal override CallResult Call()
        {
            ROS.Debug()("Called PeerConnDisconnCallback");
            SingleSubscriberPublisher pub = new SingleSubscriberPublisher(sub_link);
//            Logger.LogDebug($"Callback: Name: {pub.SubscriberName} Topic: {pub.Topic}");
            callback(pub);
            return CallResult.Success;
        }

        public override void AddToCallbackQueue(ISubscriptionCallbackHelper helper, RosMessage msg, bool nonconst_need_copy, ref bool was_full, TimeData receipt_time)
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
