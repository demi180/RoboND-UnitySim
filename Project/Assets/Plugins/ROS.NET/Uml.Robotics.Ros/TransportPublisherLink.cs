using System;
using System.Collections.Generic;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class TransportPublisherLink : PublisherLink, IDisposable
    {
//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<TransportPublisherLink>();
        public Connection connection;
        public bool dropping;
        private bool needs_retry;
        private DateTime next_retry;
        private TimeSpan retry_period;
        private WrappedTimer retry_timer;

        public TransportPublisherLink(Subscription parent, string xmlrpc_uri) : base(parent, xmlrpc_uri)
        {
            needs_retry = false;
            dropping = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            dropping = true;
            if (retry_timer != null)
            {
                ROS.timerManager.RemoveTimer(ref retry_timer);
            }
            connection.drop(Connection.DropReason.Destructing);
        }

        #endregion

        public bool initialize(Connection connection)
        {
//            Logger.LogDebug("Init transport publisher link: " + parent.name);
            this.connection = connection;
            connection.DroppedEvent += onConnectionDropped;
            if (connection.transport.getRequiresHeader())
            {
                connection.setHeaderReceivedCallback(onHeaderReceived);
                IDictionary<string, string> header = new Dictionary<string, string>();

                header["topic"] = parent.name;
                header["md5sum"] = parent.md5sum;
                header["callerid"] = ThisNode.Name;
                header["type"] = parent.datatype;
                header["tcp_nodelay"] = "1";
                connection.writeHeader(header, onHeaderWritten);
            }
            else
            {
                connection.read(4, onMessageLength);
            }
            return true;
        }

        public override void drop()
        {
            dropping = true;
            connection.drop(Connection.DropReason.Destructing);
            if (parent != null)
                parent.removePublisherLink(this);
            else
            {
//                Logger.LogDebug("Last publisher link removed");
            }
        }

        private void onConnectionDropped(Connection conn, Connection.DropReason reason)
        {
//            Logger.LogDebug("TransportPublisherLink: onConnectionDropped -- " + reason);

            if (dropping || conn != connection)
                return;
            if (reason == Connection.DropReason.TransportDisconnect)
            {
                needs_retry = true;
                next_retry = DateTime.UtcNow.Add(retry_period);
                if (retry_timer == null)
                {
                    retry_timer = ROS.timerManager.StartTimer(onRetryTimer, 100);
                }
                else
                {
                    retry_timer.Restart();
                }
            }
            else
            {
                if (reason == Connection.DropReason.HeaderError)
                {
//                    Logger.LogError("Error in the Header: " +
//                                    (parent != null ? parent.name : "unknown"));
                }
                drop();
            }
        }

        private bool onHeaderReceived(Connection conn, Header header)
        {
            if (conn != connection)
                return false;
            if (!setHeader(header))
            {
                drop();
                return false;
            }
            if (retry_timer != null)
                ROS.timerManager.RemoveTimer(ref retry_timer);
            connection.read(4, onMessageLength);
            return true;
        }

        public void handleMessage<T>(T m, bool ser, bool nocopy) where T : RosMessage, new()
        {
            stats.bytesReceived += m.Serialized.Length;
            stats.messagesReceived++;
            m.connection_header = getHeader().Values;
            if (parent != null)
                stats.drops += parent.handleMessage(m, ser, nocopy, connection.header.Values, this);
            else
                Console.WriteLine($"{nameof(parent)} is null");
        }

        private bool onHeaderWritten(Connection conn)
        {
            return true;
        }

        private bool onMessageLength(Connection conn, byte[] buffer, int size, bool success)
        {
            if (retry_timer != null)
                ROS.timerManager.RemoveTimer(ref retry_timer);
            if (!success)
            {
                if (connection != null)
                    connection.read(4, onMessageLength);
                return true;
            }
            if (conn != connection || size != 4)
                return false;
            int len = BitConverter.ToInt32(buffer, 0);
            int lengthLimit = 1000000000;
            if (len > lengthLimit)
            {
//                Logger.LogError($"TransportPublisherLink length exceeds limit of {lengthLimit}. Dropping connection");
                drop();
                return false;
            }
            connection.read(len, onMessage);
            return true;
        }

        private bool onMessage(Connection conn, byte[] buffer, int size, bool success)
        {
            if (!success || conn == null || conn != connection)
                return false;

            if (success)
            {
                RosMessage msg = RosMessage.Generate(parent.msgtype);
                msg.Serialized = buffer;
                msg.connection_header = getHeader().Values;
                handleMessage(msg, true, false);
            }
            if (success || !connection.transport.getRequiresHeader())
                connection.read(4, onMessageLength);
            return true;
        }

        private void onRetryTimer(object o)
        {
//            Logger.LogDebug("TransportPublisherLink: onRetryTimer");
            if (dropping)
                return;

            if (needs_retry && DateTime.UtcNow.Subtract(next_retry).TotalMilliseconds < 0)
            {
                retry_period =
                    TimeSpan.FromSeconds((retry_period.TotalSeconds > 20) ? 20 : (2 * retry_period.TotalSeconds));
                needs_retry = false;
                TcpTransport old_transport = connection.transport;
                string host = old_transport.connectedHost;
                int port = old_transport.connectedPort;

                TcpTransport transport = new TcpTransport();
                if (transport.connect(host, port))
                {
                    Connection conn = new Connection();
                    conn.initialize(transport, false, null);
                    initialize(conn);
                    ConnectionManager.Instance.AddConnection(conn);
                }
            }
        }
    }
}
