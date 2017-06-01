using System;
using System.Collections.Generic;
using System.Linq;
using Uml.Robotics.XmlRpc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Uml.Robotics.Ros
{
    public class Subscription
    {
        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<Subscription>();

        private List<ICallbackInfo> callbacks = new List<ICallbackInfo>();
        private bool _dropped;
        private bool shutting_down;

        private object callbacks_mutex = new object();

        private Dictionary<PublisherLink, LatchInfo> latched_messages = new Dictionary<PublisherLink, LatchInfo>();

        private object md5sum_mutex = new object();

        private int nonconst_callbacks;
        private List<PublisherLink> publisher_links = new List<PublisherLink>();
        private object publisher_links_mutex = new object(), shutdown_mutex = new object();

        private List<PendingConnection> pendingConnections = new List<PendingConnection>();

        public readonly string name;
        public string md5sum;
        public readonly string datatype;
        public readonly string msgtype;


        public Subscription(string name, string md5sum, string dataType)
        {
            this.name = name;
            this.md5sum = md5sum;
            this.datatype = dataType;
            this.msgtype = dataType;
        }

        public bool IsDropped
        {
            get { return _dropped; }
        }

        public int NumPublishers
        {
            get
            {
                lock (publisher_links_mutex)
                {
                    return publisher_links.Count;
                }
            }
        }

        public int NumCallbacks
        {
            get
            {
                lock (callbacks_mutex)
                {
                    return callbacks.Count;
                }
            }
        }

        public void shutdown()
        {
            lock (shutdown_mutex)
            {
                shutting_down = true;
            }
            drop();
        }

        public XmlRpcValue getStats()
        {
            var stats = new XmlRpcValue();
            stats.Set(0, name);
            var conn_data = new XmlRpcValue();
            conn_data.SetArray(0);
            lock (publisher_links_mutex)
            {
                int cidx = 0;
                foreach (PublisherLink link in publisher_links)
                {
                    XmlRpcValue v = new XmlRpcValue();
                    var s = link.stats;
                    v.Set(0, link.ConnectionID);
                    v.Set(1, s.bytesReceived);
                    v.Set(2, s.messagesReceived);
                    v.Set(3, s.drops);
                    v.Set(4, 0);
                    conn_data.Set(cidx++, v);
                }
            }
            stats.Set(1, conn_data);
            return stats;
        }

        public void getInfo(XmlRpcValue info)
        {
            lock (publisher_links_mutex)
            {
                //Logger.LogDebug("SUB: getInfo with " + publisher_links.Count + " publinks in list");
                foreach (PublisherLink c in publisher_links)
                {
                    //Logger.LogDebug("PUB: adding a curr_info to info!");
                    var curr_info = new XmlRpcValue();
                    curr_info.Set(0, (int) c.ConnectionID);
                    curr_info.Set(1, c.XmlRpcUri);
                    curr_info.Set(2, "i");
                    curr_info.Set(3, c.TransportType);
                    curr_info.Set(4, name);
                    //Logger.LogDebug("PUB curr_info DUMP:\n\t");
                    //curr_info.Dump();
                    info.Set(info.Count, curr_info);
                }
                //Logger.LogDebug("SUB: outgoing info is of type: " + info.Type + " and has size: " + info.Size);
            }
        }

        public void drop()
        {
            if (!_dropped)
            {
                _dropped = true;
                dropAllConnections();
            }
        }

        public void dropAllConnections()
        {
            List<PublisherLink> subscribers;
            lock (publisher_links_mutex)
            {
                subscribers = publisher_links;
                publisher_links = new List<PublisherLink>();
            }
            foreach (PublisherLink it in subscribers)
            {
                it.drop();
            }
        }

        public bool urisEqual(string uri1, string uri2)
        {
            if (uri1 == null)
            {
                throw new ArgumentNullException(nameof(uri1));
            }
            if (uri2 == null)
            {
                throw new ArgumentNullException(nameof(uri2));
            }

            string h1, h2;
            int p1, p2;
            return Network.SplitUri(uri1, out h1, out p1) && Network.SplitUri(uri2, out h2, out p2) && h1 == h2 && p1 == p2;
        }

        public void removePublisherLink(PublisherLink pub)
        {
            lock (publisher_links_mutex)
            {
                if (publisher_links.Contains(pub))
                {
                    publisher_links.Remove(pub);
                }
                if (pub.Latched)
                    latched_messages.Remove(pub);
            }
        }

        public void addPublisherLink(PublisherLink pub)
        {
            publisher_links.Add(pub);
        }

        public bool pubUpdate(IEnumerable<string> publisherUris)
        {
            using (Logger.BeginScope(nameof(pubUpdate)))
            {
                lock (shutdown_mutex)
                {
                    if (shutting_down || _dropped)
                        return false;
                }

                bool retval = true;

                Logger.LogDebug("Publisher update for [" + name + "]");

                var additions = new List<string>();
                List<PublisherLink> subtractions;
                lock (publisher_links_mutex)
                {
                    subtractions = publisher_links.Where(x => !publisherUris.Any(u => urisEqual(x.XmlRpcUri, u))).ToList();
                    foreach (string uri in publisherUris)
                    {
                        bool found = publisher_links.Any(spc => urisEqual(uri, spc.XmlRpcUri));
                        if (found)
                            continue;

                        lock (pendingConnections)
                        {
                            if (pendingConnections.Any(pc => urisEqual(uri, pc.RemoteUri)))
                            {
                                found = true;
                            }

                            if (!found)
                                additions.Add(uri);
                        }
                    }
                }
                foreach (PublisherLink link in subtractions)
                {
                    if (link.XmlRpcUri != XmlRpcManager.Instance.Uri)
                    {
                        Logger.LogDebug("Disconnecting from publisher [" + link.CallerID + "] of topic [" + name +
                                    "] at [" + link.XmlRpcUri + "]");
                        link.drop();
                    }
                    else
                    {
                        Logger.LogWarning("Cannot disconnect from self for topic: " + name);
                    }
                }

                foreach (string i in additions)
                {
                    if (XmlRpcManager.Instance.Uri != i)
                    {
                        retval &= NegotiateConnection(i);
                        //Logger.LogDebug("NEGOTIATINGING");
                    }
                    else
                        Logger.LogInformation("Skipping myself (" + name + ", " + XmlRpcManager.Instance.Uri + ")");
                }
                return retval;
            }
        }

        public bool NegotiateConnection(string xmlRpcUri)
        {
            int protos = 0;
            XmlRpcValue tcpros_array = new XmlRpcValue(), protos_array = new XmlRpcValue(), Params = new XmlRpcValue();
            tcpros_array.Set(0, "TCPROS");
            protos_array.Set(protos++, tcpros_array);
            Params.Set(0, ThisNode.Name);
            Params.Set(1, name);
            Params.Set(2, protos_array);
            if (!Network.SplitUri(xmlRpcUri, out string peerHost, out int peerPort))
            {
                Logger.LogError("Bad xml-rpc URI: [" + xmlRpcUri + "]");
                return false;
            }

            var client = new XmlRpcClient(peerHost, peerPort);
            var requestTopicTask = client.ExecuteAsync("requestTopic", Params);
            if (requestTopicTask.IsFaulted)
            {
                Logger.LogError("Failed to contact publisher [" + peerHost + ":" + peerPort + "] for topic [" + name + "]");
                return false;

            }

            Logger.LogDebug("Began asynchronous xmlrpc connection to http://" + peerHost + ":" + peerPort +
                            "/ for topic [" + name + "]");

            var conn = new PendingConnection(client, requestTopicTask, xmlRpcUri);
            lock (pendingConnections)
            {
                pendingConnections.Add(conn);
                requestTopicTask.ContinueWith(t => PendingConnectionDone(conn, t));
            }

            return true;
        }

        private void PendingConnectionDone(PendingConnection conn, Task<XmlRpcCallResult> callTask)
        {
            lock (pendingConnections)
            {
                pendingConnections.Remove(conn);
            }

            using (Logger.BeginScope (nameof(PendingConnectionDone)))
            {
                if (callTask.IsFaulted)
                {
                    Logger.LogWarning($"Negotiating for {name} has failed (Error: {callTask.Exception.Message}).");
                    return;
                }

                if (!callTask.Result.Success)
                {
                    Logger.LogWarning($"Negotiating for {name} has failed. XML-RCP call failed.");
                    return;
                }

                var resultValue = callTask.Result.Value;

                lock (shutdown_mutex)
                {
                    if (shutting_down || _dropped)
                        return;
                }

                var proto = new XmlRpcValue();
                if (!XmlRpcManager.Instance.ValidateXmlRpcResponse("requestTopic", resultValue, proto))
                {
                    Logger.LogWarning($"Negotiating for {name} has failed.");
                    return;
                }

                string peerHost = conn.Client.Host;
                int peerPort = conn.Client.Port;
                string xmlrpcUri = "http://" + peerHost + ":" + peerPort + "/";
                if (proto.Count == 0)
                {
                    Logger.LogDebug(
                        $"Could not agree on any common protocols with [{xmlrpcUri}] for topic [{name}]"
                    );
                    return;
                }
                if (proto.Type != XmlRpcType.Array)
                {
                    Logger.LogWarning($"Available protocol info returned from {xmlrpcUri} is not a list.");
                    return;
                }

                string protoName = proto[0].GetString();
                if (protoName == "UDPROS")
                {
                    Logger.LogError("UDP is currently not supported. Use TCPROS instead.");
                }
                else if (protoName == "TCPROS")
                {
                    if (proto.Count != 3 || proto[1].Type != XmlRpcType.String || proto[2].Type != XmlRpcType.Int)
                    {
                        Logger.LogWarning("TcpRos Publisher should implement string, int as parameter");
                        return;
                    }

                    string pubHost = proto[1].GetString();
                    int pubPort = proto[2].GetInt();
                    Logger.LogDebug($"Connecting via tcpros to topic [{name}] at host [{pubHost}:{pubPort}]");

                    var transport = new TcpTransport(PollManager.Instance.poll_set) { _topic = name };
                    if (transport.connect(pubHost, pubPort))
                    {
                        var connection = new Connection();
                        var pubLink = new TransportPublisherLink(this, xmlrpcUri);

                        connection.initialize(transport, false, null);
                        pubLink.initialize(connection);

                        ConnectionManager.Instance.AddConnection(connection);

                        lock (publisher_links_mutex)
                        {
                            addPublisherLink(pubLink);
                        }

                        Logger.LogDebug($"Connected to publisher of topic [{name}] at  [{pubHost}:{pubPort}]");
                    }
                    else
                    {
                        Logger.LogError($"Failed to connect to publisher of topic [{name}] at [{pubHost}:{pubPort}]");
                    }
                }
                else
                {
                    Logger.LogError("The XmlRpc Server does not provide a supported protocol.");
                }
            }
        }

        public void headerReceived(PublisherLink link, Header header)
        {
            lock (md5sum_mutex)
            {
                if (md5sum == "*")
                    md5sum = link.md5sum;
            }
        }

        internal long handleMessage(
            RosMessage msg,
            bool ser,
            bool nocopy,
            IDictionary<string, string> connection_header,
            PublisherLink link
        )
        {
            RosMessage t = null;
            long drops = 0;
            TimeData receipt_time = ROS.GetTime().data;
            if (msg.Serialized != null) // will be null if self-subscribed
                msg.Deserialize(msg.Serialized);

            lock (callbacks_mutex)
            {
                foreach (ICallbackInfo info in callbacks)
                {
                    string ti = info.helper.type;
                    if (nocopy || ser)
                    {
                        t = msg;
                        t.connection_header = msg.connection_header;
                        t.Serialized = null;
                        bool was_full = false;
                        bool nonconst_need_copy = callbacks.Count > 1;
                        info.subscription_queue.AddToCallbackQueue(info.helper, t, nonconst_need_copy, ref was_full, receipt_time);
                        if (was_full)
                            ++drops;
                        else
                            info.callback.AddCallback(info.subscription_queue);
                    }
                }
            }

            if (t != null && link.Latched)
            {
                LatchInfo li = new LatchInfo
                {
                    message = t,
                    link = link,
                    connection_header = connection_header,
                    receipt_time = receipt_time
                };
                if (latched_messages.ContainsKey(link))
                    latched_messages[link] = li;
                else
                    latched_messages.Add(link, li);
            }

            return drops;
        }

        public void Dispose()
        {
            shutdown();
        }

        internal bool addCallback(
            ISubscriptionCallbackHelper helper,
            string md5sum,
            ICallbackQueue queue,
            int queue_size,
            bool allow_concurrent_callbacks,
            string topiclol
        )
        {
            lock (md5sum_mutex)
            {
                if (this.md5sum == "*" && md5sum != "*")
                    this.md5sum = md5sum;
            }

            if (md5sum != "*" && md5sum != this.md5sum)
                return false;

            lock (callbacks_mutex)
            {
                ICallbackInfo info = new ICallbackInfo
                {
                    helper = helper,
                    callback = queue,
                    subscription_queue = new Callback(helper.Callback.SendEvent, topiclol, queue_size, allow_concurrent_callbacks)
                };

                //if (!helper.isConst())
                //{
                ++nonconst_callbacks;
                //}

                callbacks.Add(info);

                if (latched_messages.Count > 0)
                {
                    string ti = info.helper.type;
                    lock (publisher_links_mutex)
                    {
                        foreach (PublisherLink link in publisher_links)
                        {
                            if (link.Latched)
                            {
                                if (latched_messages.ContainsKey(link))
                                {
                                    LatchInfo latch_info = latched_messages[link];
                                    bool was_full = false;
                                    bool nonconst_need_copy = callbacks.Count > 1;
                                    info.subscription_queue.AddToCallbackQueue(info.helper, latched_messages[link].message, nonconst_need_copy, ref was_full, ROS.GetTime().data);
                                    if (!was_full)
                                        info.callback.AddCallback(info.subscription_queue);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void removeCallback(ISubscriptionCallbackHelper helper)
        {
            lock (callbacks_mutex)
            {
                foreach (ICallbackInfo info in callbacks)
                {
                    if (info.helper == helper)
                    {
                        info.subscription_queue.Clear();
                        info.callback.RemoveById(info.subscription_queue.Uid);
                        callbacks.Remove(info);
                        //if (!helper.isConst())
                        --nonconst_callbacks;
                        break;
                    }
                }
            }
        }

        public void addLocalConnection(Publication pub)
        {
            lock (publisher_links_mutex)
            {
                if (_dropped)
                    return;

                Logger.LogInformation("Creating intraprocess link for topic [{0}]", name);

                var pub_link = new LocalPublisherLink(this, XmlRpcManager.Instance.Uri);
                var sub_link = new LocalSubscriberLink(pub);
                pub_link.setPublisher(sub_link);
                sub_link.SetSubscriber(pub_link);

                addPublisherLink(pub_link);
                pub.addSubscriberLink(sub_link);
            }
        }

        public void getPublishTypes(ref bool ser, ref bool nocopy, string typeInfo)
        {
            lock (callbacks_mutex)
            {
                foreach (ICallbackInfo info in callbacks)
                {
                    if (info.helper.type == typeInfo)
                        nocopy = true;
                    else
                        ser = true;
                    if (nocopy && ser)
                        return;
                }
            }
        }

        private class ICallbackInfo
        {
            public ICallbackQueue callback;
            public ISubscriptionCallbackHelper helper;
            public CallbackInterface subscription_queue;
        }

        //private class CallbackInfo<M>
        //    : ICallbackInfo where M : RosMessage, new()
        //{
        //    public CallbackInfo()
        //    {
        //        helper = new SubscriptionCallbackHelper<M>(new M().MessageType);
        //    }
        //}

        private class LatchInfo
        {
            public IDictionary<string, string> connection_header;
            public PublisherLink link;
            public RosMessage message;
            public TimeData receipt_time;
        }
    }
}
