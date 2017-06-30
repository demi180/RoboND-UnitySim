using System;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class TransportSubscriberLink : SubscriberLink, IDisposable
    {
//        ILogger Logger { get; } = ApplicationLogging.CreateLogger<TransportSubscriberLink>();
        Connection connection;
        bool headerWritten;
        int maxQueue;
        Queue<MessageAndSerializerFunc> outbox = new Queue<MessageAndSerializerFunc>();
        new Publication parent;
        bool queueFull;
        bool writingMessage;

        public TransportSubscriberLink()
        {
            writingMessage = false;
            headerWritten = false;
            queueFull = false;
        }

        public void Dispose()
        {
            Drop();
        }

        public bool Initialize(Connection connection)
        {
			if ( parent != null )
				UnityEngine.Debug.Log ( "l1" );
//                Logger.LogDebug("Init transport subscriber link: " + parent.Name);
            this.connection = connection;
            connection.DroppedEvent += OnConnectionDropped;
            return true;
        }

        public bool HandleHeader(Header header)
        {
            if (!header.Values.ContainsKey("topic"))
            {
                string msg = "Header from subscriber did not have the required element: topic";
//                Logger.LogWarning(msg);
                connection.sendHeaderError(ref msg);
                return false;
            }
            string name = (string) header.Values["topic"];
            string client_callerid = (string) header.Values["callerid"];
            Publication pt = TopicManager.Instance.lookupPublication(name);
            if (pt == null)
            {
                string msg = "received a connection for a nonexistent topic [" + name + "] from [" +
                             connection.transport + "] [" + client_callerid + "]";
//                Logger.LogWarning(msg);
                connection.sendHeaderError(ref msg);
                return false;
            }
            string error_message = "";
            if (!pt.validateHeader(header, ref error_message))
            {
                connection.sendHeaderError(ref error_message);
//                Logger.LogError(error_message);
                return false;
            }
            destination_caller_id = client_callerid;
            connection_id = ConnectionManager.Instance.GetNewConnectionId();
            name = pt.Name;
            parent = pt;
            lock (parent)
            {
                maxQueue = parent.MaxQueue;
            }

            var m = new Dictionary<string, string>();
            m["type"] = pt.DataType;
            m["md5sum"] = pt.Md5sum;
            m["message_definition"] = pt.MessageDefinition;
            m["callerid"] = ThisNode.Name;
            m["latching"] = Convert.ToString(pt.Latch);
            connection.writeHeader(m, OnHeaderWritten);
            pt.addSubscriberLink(this);
//            Logger.LogDebug("Finalize transport subscriber link for " + name);
            return true;
        }

        internal override void EnqueueMessage(MessageAndSerializerFunc holder)
        {
            lock (outbox)
            {
                if (maxQueue > 0 && outbox.Count >= maxQueue)
                {
                    outbox.Dequeue();
                    queueFull = true;
                }
                else
                {
                    queueFull = false;
                }
                outbox.Enqueue(holder);
            }
            StartMessageWrite(false);
        }

        public override void Drop()
        {
            if (connection.sendingHeaderError)
                connection.DroppedEvent -= OnConnectionDropped;
            else
                connection.drop(Connection.DropReason.Destructing);
        }

        private void OnConnectionDropped(Connection conn, Connection.DropReason reason)
        {
            if (conn != connection || parent == null)
                return;

            lock (parent)
            {
                parent.removeSubscriberLink(this);
            }
        }

        private bool OnHeaderWritten(Connection conn)
        {
            headerWritten = true;
            StartMessageWrite(true);
            return true;
        }

        private bool OnMessageWritten(Connection conn)
        {
            writingMessage = false;
            StartMessageWrite(true);
            return true;
        }

        private void StartMessageWrite(bool immediateWrite)
        {
            MessageAndSerializerFunc holder = null;
            if (writingMessage || !headerWritten)
                return;

            lock (outbox)
            {
                if (outbox.Count > 0)
                {
                    writingMessage = true;
                    holder = outbox.Dequeue();
                }
                if (outbox.Count < maxQueue)
                    queueFull = false;
            }

            if (holder != null)
            {
                if (holder.msg.Serialized == null)
                    holder.msg.Serialized = holder.serfunc();
                byte[] outbuf = new byte[holder.msg.Serialized.Length + 4];
                Array.Copy(holder.msg.Serialized, 0, outbuf, 4, holder.msg.Serialized.Length);
                Array.Copy(BitConverter.GetBytes(holder.msg.Serialized.Length), outbuf, 4);
                stats.messagesSent++;
                //Logger.LogDebug("Message backlog = " + (triedtosend - stats.messages_sent));
                stats.bytesSent += outbuf.Length;
                stats.messageDataSent += outbuf.Length;
                connection.write(outbuf, outbuf.Length, OnMessageWritten, immediateWrite);
            }
        }
    }
}
