using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class ConnectionManager
    {
        public static ConnectionManager Instance
        {
            get { return instance.Value; }
        }

        internal static void Terminate()
        {
            Instance.Shutdown();
        }


        internal static void Reset()
        {
            instance = new Lazy<ConnectionManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private uint connection_id_counter;
        private object connection_id_counter_mutex = new object();
        private List<Connection> connections = new List<Connection>();
        private object connections_mutex = new object();
        private List<Connection> dropped_connections = new List<Connection>();
        private object dropped_connections_mutex = new object();
        private TcpListener tcpserver_transport;
        private WrappedTimer acceptor;
//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<ConnectionManager>();
        private static Lazy<ConnectionManager> instance = new Lazy<ConnectionManager>(LazyThreadSafetyMode.ExecutionAndPublication);


        public int TCPPort
        {
            get
            {
                if (tcpserver_transport == null || tcpserver_transport.LocalEndpoint == null)
                    return -1;
                return ((IPEndPoint) tcpserver_transport.LocalEndpoint).Port;
            }
        }


        public uint GetNewConnectionId()
        {
            lock (connection_id_counter_mutex)
            {
                return connection_id_counter++;
            }
        }


        public void AddConnection(Connection connection)
        {
            lock (connections_mutex)
            {
                connections.Add(connection);
                connection.DroppedEvent += OnConnectionDropped;
            }
        }


        public void Clear(Connection.DropReason reason)
        {
            RemoveDroppedConnections();
            List<Connection> local_connections = null;
            lock (connections_mutex)
            {
                local_connections = new List<Connection>(connections);
                connections.Clear();
            }
            foreach (Connection c in local_connections)
            {
                if (!c.dropped)
                    c.drop(reason);
            }
            lock (dropped_connections_mutex)
                dropped_connections.Clear();
        }


        public void Shutdown()
        {
            acceptor.Stop();

            if (tcpserver_transport != null)
            {
                tcpserver_transport.Stop();
                tcpserver_transport = null;
            }
            PollManager.Instance.RemovePollThreadListener(RemoveDroppedConnections);

            Clear(Connection.DropReason.Destructing);
        }


        public void TcpRosAcceptConnection(TcpTransport transport)
        {
            Connection conn = new Connection();
            AddConnection(conn);
            conn.initialize(transport, true, OnConnectionHeaderReceived);
        }


        public bool OnConnectionHeaderReceived(Connection conn, Header header)
        {
            bool ret = false;
            if (header.Values.ContainsKey("topic"))
            {
                TransportSubscriberLink sub_link = new TransportSubscriberLink();
                ret = sub_link.Initialize(conn);
                ret &= sub_link.HandleHeader(header);
            }
            else if (header.Values.ContainsKey("service"))
            {
                IServiceClientLink iscl = new IServiceClientLink();
                ret = iscl.initialize(conn);
                ret &= iscl.handleHeader(header);
            }
            else
            {
//                Logger.LogWarning("Got a connection for a type other than topic or service from [" + conn.RemoteString +
//                              "].");
                return false;
            }
            //Logger.LogDebug("CONNECTED [" + val + "]. WIN.");
            return ret;
        }


        public void CheckAndAccept(object nothing)
        {
            while (tcpserver_transport != null && tcpserver_transport.Pending())
            {
                TcpRosAcceptConnection(new TcpTransport(tcpserver_transport.AcceptSocketAsync().Result, PollManager.Instance.poll_set));
            }
        }


        public void Start()
        {
            PollManager.Instance.AddPollThreadListener(RemoveDroppedConnections);

            tcpserver_transport = new TcpListener(IPAddress.Any, Network.TcpRosServerPort);
            tcpserver_transport.Start(10);
            acceptor = ROS.timerManager.StartTimer(CheckAndAccept, 100, 100);

        }


        private void OnConnectionDropped(Connection conn, Connection.DropReason r)
        {
            lock (dropped_connections_mutex)
                dropped_connections.Add(conn);
        }


        private void RemoveDroppedConnections()
        {
            List<Connection> localDropped = null;
            lock (dropped_connections_mutex)
            {
                localDropped = new List<Connection>(dropped_connections);
                dropped_connections.Clear();
            }
            lock (connections_mutex)
            {
                foreach (Connection c in localDropped)
                {
//                    Logger.LogDebug("Removing dropped connection: " + c.CallerID);
                    connections.Remove(c);
                }
            }
        }
    }
}
