//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Uml.Robotics.XmlRpc;


namespace Uml.Robotics.Ros
{
    public class TopicManager
    {
        public delegate byte[] SerializeFunc();

        public static TopicManager Instance
        {
            get { return instance.Value; }
        }

        private static Lazy<TopicManager> instance = new Lazy<TopicManager>(LazyThreadSafetyMode.ExecutionAndPublication);
//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<TopicManager>();
        private List<Publication> advertisedTopics = new List<Publication>();
        private object advertisedTopicsMutex = new object();
        private bool shuttingDown;
        private object shuttingDownMutex = new object();
        private object subcriptionsMutex = new object();
        private List<Subscription> subscriptions = new List<Subscription>();


        internal static void Terminate()
        {
            Instance.Shutdown();
        }

        internal static void Reset()
        {
            instance = new Lazy<TopicManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        }


        /// <summary>
        ///     Binds the XmlRpc requests to callback functions, signal to start
        /// </summary>
        public void Start()
        {
            lock (shuttingDownMutex)
            {
                shuttingDown = false;

                XmlRpcManager.Instance.Bind("publisherUpdate", publisherUpdateCallback);
                XmlRpcManager.Instance.Bind("requestTopic", requestTopicCallback);
                XmlRpcManager.Instance.Bind("getBusStats", getBusStatsCallback);
                XmlRpcManager.Instance.Bind("getBusInfo", getBusInfoCallback);
                XmlRpcManager.Instance.Bind("getSubscriptions", getSubscriptionsCallback);
                XmlRpcManager.Instance.Bind("getPublications", getPublicationsCallback);
            }
        }


        /// <summary>
        ///     unbinds the XmlRpc requests to callback functions, signal to shutdown
        /// </summary>
        public void Shutdown()
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                    return;

                lock (subcriptionsMutex)
                {
                    shuttingDown = true;
                }

                XmlRpcManager.Instance.Unbind("publisherUpdate");
                XmlRpcManager.Instance.Unbind("requestTopic");
                XmlRpcManager.Instance.Unbind("getBusStats");
                XmlRpcManager.Instance.Unbind("getBusInfo");
                XmlRpcManager.Instance.Unbind("getSubscriptions");
                XmlRpcManager.Instance.Unbind("getPublications");

                bool failedOnceToUnadvertise = false;
                lock (advertisedTopicsMutex)
                {
                    foreach (Publication p in advertisedTopics)
                    {
                        if (!p.Dropped && !failedOnceToUnadvertise)
                        {
                            failedOnceToUnadvertise = !unregisterPublisher(p.Name);
                        }
                        p.drop();
                    }
                    advertisedTopics.Clear();
                }

                bool failedOnceToUnsubscribe = false;
                lock (subcriptionsMutex)
                {
                    foreach (Subscription s in subscriptions)
                    {
                        if (!s.IsDropped && !failedOnceToUnsubscribe)
                        {
                            failedOnceToUnsubscribe = !unregisterSubscriber(s.name);
                        }
                        s.shutdown();
                    }
                    subscriptions.Clear();
                }
            }
        }


        /// <summary>
        ///     gets the list of advertised topics.
        /// </summary>
        /// <param name="topics">List of topics to update</param>
        public void getAdvertisedTopics(out string[] topics)
        {
            lock (advertisedTopicsMutex)
            {
                topics = advertisedTopics.Select(a => a.Name).ToArray();
            }
        }


        /// <summary>
        ///     gets the list of subscribed topics.
        /// </summary>
        /// <param name="topics"></param>
        public void getSubscribedTopics(out string[] topics)
        {
            lock (subcriptionsMutex)
            {
                topics = subscriptions.Select(s => s.name).ToArray();
            }
        }


        /// <summary>
        ///     Looks up all current publishers on a given topic
        /// </summary>
        /// <param name="topic">Topic name to look up</param>
        /// <returns></returns>
        public Publication lookupPublication(string topic)
        {
            lock (advertisedTopicsMutex)
            {
                return lookupPublicationWithoutLock(topic);
            }
        }


        /// <summary>
        ///     Checks if the given topic is valid.
        /// </summary>
        /// <typeparam name="T">Advertise Options </typeparam>
        /// <param name="ops"></param>
        /// <returns></returns>
        private bool isValid<T>(AdvertiseOptions<T> ops) where T : RosMessage, new()
        {
            if (ops.dataType == "*")
                throw new Exception("Advertising with * as the datatype is not allowed.  Topic [" + ops.topic + "]");
            if (ops.md5Sum == "*")
                throw new Exception("Advertising with * as the md5sum is not allowed.  Topic [" + ops.topic + "]");
            if (ops.md5Sum == "")
                throw new Exception("Advertising on topic [" + ops.topic + "] with an empty md5sum");
            if (ops.dataType == "")
                throw new Exception("Advertising on topic [" + ops.topic + "] with an empty datatype");
            if (string.IsNullOrEmpty(ops.messageDefinition))
            {
//                this.Logger.LogWarning(
//                    "Advertising on topic [" + ops.topic +
//                     "] with an empty message definition. Some tools may not work correctly"
//                );
            }
            return true;
        }


        /// <summary>
        ///     Register as a publisher on a topic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ops"></param>
        /// <param name="callbacks"></param>
        /// <returns></returns>
        public bool advertise<T>(AdvertiseOptions<T> ops, SubscriberCallbacks callbacks) where T : RosMessage, new()
        {
            if (!isValid(ops))
                return false;

            Publication pub = null;
            lock (advertisedTopicsMutex)
            {
                if (shuttingDown)
                    return false;
                pub = lookupPublicationWithoutLock(ops.topic);
                if (pub != null)
                {
                    if (pub.Md5sum != ops.md5Sum)
                    {
//                        this.Logger.LogError(
//                            "Tried to advertise on topic [{0}] with md5sum [{1}] and datatype [{2}], but the topic is already advertised as md5sum [{3}] and datatype [{4}]",
//                            ops.topic, ops.md5Sum,
//                            ops.dataType, pub.Md5sum, pub.DataType
//                        );
                        return false;
                    }
                }
                else
                    pub = new Publication(ops.topic, ops.dataType, ops.md5Sum, ops.messageDefinition, ops.queueSize,
                        ops.latch, ops.hasHeader);
                pub.addCallbacks(callbacks);
                advertisedTopics.Add(pub);
            }

            bool found = false;
            Subscription sub = null;
            lock (subcriptionsMutex)
            {
                foreach (Subscription s in subscriptions)
                {
                    if (s.name == ops.topic && md5sumsMatch(s.md5sum, ops.md5Sum) && !s.IsDropped)
                    {
                        found = true;
                        sub = s;
                        break;
                    }
                }
            }

            if (found)
                sub.addLocalConnection(pub);

            var args = new XmlRpcValue(ThisNode.Name, ops.topic, ops.dataType, XmlRpcManager.Instance.Uri);
            var result = new XmlRpcValue();
            var payload = new XmlRpcValue();

            if (!Master.execute("registerPublisher", args, result, payload, true))
            {
//                this.Logger.LogError("RPC \"registerService\" for service " + ops.topic + " failed.");
                return false;
            }

            return true;
        }


        public void subscribe(SubscribeOptions ops)
        {
            lock (subcriptionsMutex)
            {
                if (addSubCallback(ops))
                    return;
                if (shuttingDown)
                    return;
            }
            if (string.IsNullOrEmpty(ops.md5sum))
                throw subscribeFail(ops, "with an empty md5sum");
            if (string.IsNullOrEmpty(ops.datatype))
                throw subscribeFail(ops, "with an empty datatype");
            if (ops.helper == null)
                throw subscribeFail(ops, "without a callback");

            string md5sum = ops.md5sum;
            string datatype = ops.datatype;
            var s = new Subscription(ops.topic, md5sum, datatype);
            s.addCallback(ops.helper, ops.md5sum, ops.callback_queue, ops.queue_size, ops.allow_concurrent_callbacks, ops.topic);
            if (!registerSubscriber(s, ops.datatype))
            {
                string error = $"Couldn't register subscriber on topic [{ops.topic}]";
                s.shutdown();
//                this.Logger.LogError(error);
                throw new RosException(error);
            }

            lock (subcriptionsMutex)
            {
                subscriptions.Add(s);
            }
        }


        public Exception subscribeFail(SubscribeOptions ops, string reason)
        {
            return new Exception("Subscribing to topic [" + ops.topic + "] " + reason);
        }


        public bool unsubscribe(string topic, ISubscriptionCallbackHelper sbch)
        {
            Subscription sub = null;
            lock (subcriptionsMutex)
            {
                if (shuttingDown)
                    return false;
                foreach (Subscription s in subscriptions)
                {
                    if (s.name == topic)
                    {
                        sub = s;
                        break;
                    }
                }
            }

            if (sub == null)
                return false;

            sub.removeCallback(sbch);
            if (sub.NumCallbacks == 0)
            {
                lock (subcriptionsMutex)
                {
                    subscriptions.Remove(sub);
                }

				if ( !unregisterSubscriber ( topic ) )
					UnityEngine.Debug.LogWarning ( "no" );
//                    this.Logger.LogWarning("Couldn't unregister subscriber for topic [" + topic + "]");

                sub.shutdown();
                return true;
            }
            return true;
        }


        internal Subscription getSubscription(string topic)
        {
            lock (subcriptionsMutex)
            {
                if (shuttingDown)
                    return null;

                foreach (Subscription t in subscriptions)
                {
                    if (!t.IsDropped && t.name == topic)
                        return t;
                }
            }
            return null;
        }


        public int getNumSubscriptions()
        {
            lock (subcriptionsMutex)
            {
                return subscriptions.Count;
            }
        }


        public void publish(Publication p, RosMessage msg, SerializeFunc serfunc = null)
        {
            if (msg == null)
                return;
            if (serfunc == null)
                serfunc = msg.Serialize;
            if (p.connection_header == null)
            {
                p.connection_header = new Header {Values = new Dictionary<string, string>()};
                p.connection_header.Values["type"] = p.DataType;
                p.connection_header.Values["md5sum"] = p.Md5sum;
                p.connection_header.Values["message_definition"] = p.MessageDefinition;
                p.connection_header.Values["callerid"] = ThisNode.Name;
                p.connection_header.Values["latching"] = Convert.ToString(p.Latch);
            }

            if (!ROS.ok || shuttingDown)
                return;

            if (p.HasSubscribers || p.Latch)
            {
                bool nocopy = false;
                bool serialize = false;
                if (msg != null && msg.MessageType != "xamla/unkown")
                {
                    p.getPublishTypes(ref serialize, ref nocopy, msg.MessageType);
                }
                else
                {
                    serialize = true;
                }

                p.publish(new MessageAndSerializerFunc(msg, serfunc, serialize, nocopy));

                if (serialize)
                    PollManager.Instance.poll_set.ContinueThreads();
            }
            else
            {
                p.incrementSequence();
            }
        }


        public void incrementSequence(string topic)
        {
            Publication pub = lookupPublication(topic);
            if (pub != null)
                pub.incrementSequence();
        }


        public bool isLatched(string topic)
        {
            Publication pub = lookupPublication(topic);
            if (pub != null)
                return pub.Latch;
            return false;
        }


        public bool md5sumsMatch(string lhs, string rhs)
        {
            return (lhs == "*" || rhs == "*" || lhs == rhs);
        }


        public bool addSubCallback(SubscribeOptions ops)
        {
            bool found = false;
            bool found_topic = false;
            Subscription sub = null;

            if (shuttingDown)
                return false;

            foreach (Subscription s in subscriptions)
            {
                sub = s;
                if (!sub.IsDropped && sub.name == ops.topic)
                {
                    found_topic = true;
                    if (md5sumsMatch(ops.md5sum, sub.md5sum))
                        found = true;
                    break;
                }
            }
            if (found_topic && !found)
            {
                throw new Exception
                    ("Tried to subscribe to a topic with the same name but different md5sum as a topic that was already subscribed [" +
                     ops.datatype + "/" + ops.md5sum + " vs. " + sub.datatype + "/" +
                     sub.md5sum + "]");
            }
            if (found)
            {
                if (!sub.addCallback(ops.helper, ops.md5sum, ops.callback_queue, ops.queue_size,
                        ops.allow_concurrent_callbacks, ops.topic))
                {
                    return false;
                }
            }
            return found;
        }


        public bool requestTopic(string topic, XmlRpcValue protos, ref XmlRpcValue ret)
        {
            for (int proto_idx = 0; proto_idx < protos.Count; proto_idx++)
            {
                XmlRpcValue proto = protos[proto_idx];
                if (proto.Type != XmlRpcType.Array)
                {
//                    this.Logger.LogError("requestTopic protocol list was not a list of lists");
                    return false;
                }
                if (proto[0].Type != XmlRpcType.String)
                {
//                    this.Logger.LogError(
//                        "requestTopic received a protocol list in which a sublist did not start with a string");
                    return false;
                }

                string proto_name = proto[0].GetString();

                if (proto_name == "TCPROS")
                {
                    var tcpRosParams = new XmlRpcValue("TCPROS", Network.host, ConnectionManager.Instance.TCPPort);
                    ret.Set(0, 1);
                    ret.Set(1, "");
                    ret.Set(2, tcpRosParams);
                    return true;
                }
                if (proto_name == "UDPROS")
                {
//                    this.Logger.LogWarning("Ignoring topics with UdpRos as protocol");
                }
                else
                {
//                    this.Logger.LogWarning("An unsupported protocol was offered: [{0}]", proto_name);
                }
            }

//            this.Logger.LogError("No supported protocol was provided");
            return false;
        }


        public bool isTopicAdvertised(string topic)
        {
            return advertisedTopics.Count(o => o.Name == topic) > 0;
        }


        public bool registerSubscriber(Subscription s, string datatype)
        {
            string uri = XmlRpcManager.Instance.Uri;

            var args = new XmlRpcValue(ThisNode.Name, s.name, datatype, uri);
            var result = new XmlRpcValue();
            var payload = new XmlRpcValue();
            if (!Master.execute("registerSubscriber", args, result, payload, true))
            {
//                Logger.LogError("RPC \"registerSubscriber\" for service " + s.name + " failed.");
                return false;
            }
            var pub_uris = new List<string>();
            for (int i = 0; i < payload.Count; i++)
            {
                XmlRpcValue load = payload[i];
                string pubed = load.GetString();
                if (pubed != uri && !pub_uris.Contains(pubed))
                {
                    pub_uris.Add(pubed);
                }
            }
            bool self_subscribed = false;
            Publication pub = null;
            string sub_md5sum = s.md5sum;
            lock (advertisedTopicsMutex)
            {
                foreach (Publication p in advertisedTopics)
                {
                    pub = p;
                    string pub_md5sum = pub.Md5sum;
                    if (pub.Name == s.name && md5sumsMatch(pub_md5sum, sub_md5sum) && !pub.Dropped)
                    {
                        self_subscribed = true;
                        break;
                    }
                }
            }

            s.pubUpdate(pub_uris);
            if (self_subscribed)
                s.addLocalConnection(pub);
            return true;
        }


        public bool unregisterSubscriber(string topic)
        {
            var args = new XmlRpcValue(ThisNode.Name, topic, XmlRpcManager.Instance.Uri);
            var result = new XmlRpcValue();
            var payload = new XmlRpcValue();

            bool unregisterSuccess = false;
            try
            {
                unregisterSuccess = Master.execute("unregisterSubscriber", args, result, payload, false) && result.IsEmpty;
            }
            // Ignore exception during unregister
            catch (Exception e)
            {
                // Logger.LogError(e.Message);
            }
            return unregisterSuccess;
        }


        public bool unregisterPublisher(string topic)
        {
            var args = new XmlRpcValue(ThisNode.Name, topic, XmlRpcManager.Instance.Uri);
            var result = new XmlRpcValue();
            var payload = new XmlRpcValue();

            bool unregisterSuccess = false;
            try
            {
                unregisterSuccess = Master.execute("unregisterPublisher", args, result, payload, false) && result.IsEmpty;
            }
            // Ignore exception during unregister
            catch (Exception e)
            {
                // Logger.LogError(e.Message);
            }
            return unregisterSuccess;
        }


        public Publication lookupPublicationWithoutLock(string topic)
        {
            return advertisedTopics.FirstOrDefault(p => p.Name == topic && !p.Dropped);
        }


        public XmlRpcValue getBusStats()
        {
            var publish_stats = new XmlRpcValue();
            var subscribe_stats = new XmlRpcValue();
            var service_stats = new XmlRpcValue();

            int pidx = 0;
            lock (advertisedTopicsMutex)
            {
                publish_stats.SetArray(advertisedTopics.Count);
                foreach (Publication t in advertisedTopics)
                {
                    publish_stats.Set(pidx++, t.GetStats());
                }
            }

            int sidx = 0;
            lock (subcriptionsMutex)
            {
                subscribe_stats.SetArray(subscriptions.Count);
                foreach (Subscription t in subscriptions)
                {
                    subscribe_stats.Set(sidx++, t.getStats());
                }
            }

            // TODO: fix for services
            service_stats.SetArray(0); //service_stats.Size = 0;

            var stats = new XmlRpcValue();
            stats.Set(0, publish_stats);
            stats.Set(1, subscribe_stats);
            stats.Set(2, service_stats);
            return stats;
        }


        public XmlRpcValue getBusInfo()
        {
            var info = new XmlRpcValue();
            info.SetArray(0);
            lock (advertisedTopicsMutex)
            {
                foreach (Publication t in advertisedTopics)
                {
                    t.getInfo(info);
                }
            }
            lock (subcriptionsMutex)
            {
                foreach (Subscription t in subscriptions)
                {
                    t.getInfo(info);
                }
            }
            return info;
        }


        public void getSubscriptions(ref XmlRpcValue subs)
        {
            subs.SetArray(0);
            lock (subcriptionsMutex)
            {
                int sidx = 0;
                foreach (Subscription t in subscriptions)
                {
                    subs.Set(sidx++, new XmlRpcValue(t.name, t.datatype));
                }
            }
        }


        public void getPublications(ref XmlRpcValue pubs)
        {
            pubs.SetArray(0);
            lock (advertisedTopicsMutex)
            {
                int sidx = 0;
                foreach (Publication t in advertisedTopics)
                {
                    XmlRpcValue pub = new XmlRpcValue();
                    pub.Set(0, t.Name);
                    pub.Set(1, t.DataType);
                    pubs.Set(sidx++, pub);
                }
            }
        }


        public bool pubUpdate(string topic, List<string> pubs)
        {
//            using (this.Logger.BeginScope(nameof(pubUpdate)))
//            {
//                this.Logger.LogDebug("TopicManager is updating publishers for " + topic);

                Subscription sub = null;
                lock (subcriptionsMutex)
                {
                    if (shuttingDown)
                        return false;

                    foreach (Subscription s in subscriptions)
                    {
                        if (s.name != topic || s.IsDropped)
                            continue;
                        sub = s;
                        break;
                    }
                }
                if (sub != null)
                    return sub.pubUpdate(pubs);
//                this.Logger.LogInformation(
//                    "Request for updating publishers of topic " + topic + ", which has no subscribers."
//                );
                return false;
//            }
        }


        private void publisherUpdateCallback(XmlRpcValue parm, XmlRpcValue result)
        {
            var pubs = new List<string>();
            for (int idx = 0; idx < parm[2].Count; idx++)
                pubs.Add(parm[2][idx].GetString());
            if (pubUpdate(parm[1].GetString(), pubs))
                XmlRpcManager.ResponseInt(1, "", 0)(result);
            else
            {
                const string error = "Unknown error while handling XmlRpc call to pubUpdate";
//                this.Logger.LogError(error);
                XmlRpcManager.ResponseInt(0, error, 0)(result);
            }
        }


        private void requestTopicCallback(XmlRpcValue parm, XmlRpcValue res)
        {
            //XmlRpcValue res = XmlRpcValue.Create(ref result)
            //	, parm = XmlRpcValue.Create(ref parms);
            //result = res.instance;
            if (!requestTopic(parm[1].GetString(), parm[2], ref res))
            {
                const string error = "Unknown error while handling XmlRpc call to requestTopic";
//                this.Logger.LogError(error);
                XmlRpcManager.ResponseInt(0, error, 0)(res);
            }
        }


        private void getBusStatsCallback(XmlRpcValue parm, XmlRpcValue res)
        {
            res.Set(0, 1);
            res.Set(1, "");
            var response = getBusStats();
            res.Set(2, response);
        }


        private void getBusInfoCallback(XmlRpcValue parm, XmlRpcValue res)
        {
            res.Set(0, 1);
            res.Set(1, "");
            var response = getBusInfo();
            res.Set(2, response);
        }


        private void getSubscriptionsCallback(XmlRpcValue parm, XmlRpcValue res)
        {
            res.Set(0, 1);
            res.Set(1, "subscriptions");
            var response = new XmlRpcValue();
            getSubscriptions(ref response);
            res.Set(2, response);
        }


        private void getPublicationsCallback(XmlRpcValue parm, XmlRpcValue res)
        {
            res.Set(0, 1);
            res.Set(1, "publications");
            var response = new XmlRpcValue();
            getPublications(ref response);
            res.Set(2, response);
        }


        public bool unadvertise(string topic, SubscriberCallbacks callbacks)
        {
            Publication pub = null;
            lock (advertisedTopicsMutex)
            {
                foreach (Publication p in advertisedTopics)
                {
                    if (p.Name == topic && !p.Dropped)
                    {
                        pub = p;
                        break;
                    }
                }
            }
            if (pub == null)
                return false;

            pub.removeCallbacks(callbacks);
            lock (advertisedTopicsMutex)
            {
                if (pub.NumCallbacks == 0)
                {
                    unregisterPublisher(pub.Name);
                    pub.drop();
                    advertisedTopics.Remove(pub);
                }
            }
            return true;
        }
    }
}
