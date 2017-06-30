using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Uml.Robotics.XmlRpc;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class ServiceManager
    {
        public static ServiceManager Instance
        {
            get { return instance.Value; }
        }

//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<ServiceManager>();
        private static Lazy<ServiceManager> instance = new Lazy<ServiceManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        private ConnectionManager connectionManager;
        private PollManager pollManager;
        private List<IServicePublication> servicePublications = new List<IServicePublication>();
        private object servicePublicationsMutex = new object();
        private List<IServiceServerLink> serviceServerLinks = new List<IServiceServerLink>();
        private object serviceServerLinksMutex = new object();
        private bool shuttingDown;
        private object shuttingDownMutex = new object();
        private XmlRpcManager xmlrpcManager;


        internal static void Terminate()
        {
            Instance.Shutdown();
        }


        internal static void Reset()
        {
            instance = new Lazy<ServiceManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        }


        public void Start()
        {
            shuttingDown = false;
            pollManager = PollManager.Instance;
            connectionManager = ConnectionManager.Instance;
            xmlrpcManager = XmlRpcManager.Instance;
        }


        internal IServicePublication LookupServicePublication(string name)
        {
            lock (servicePublicationsMutex)
            {
                foreach (IServicePublication sp in servicePublications)
                {
                    if (sp.name == name)
                        return sp;
                }
            }
            return null;
        }


        internal ServiceServerLink<S> CreateServiceServerLink<S>(string service, bool persistent, string request_md5sum, string response_md5sum, IDictionary<string, string> header_values)
            where S : RosService, new()
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                    return null;
            }

            int serv_port = -1;
            string serv_host = "";
            if (!LookupService(service, ref serv_host, ref serv_port))
                return null;

            TcpTransport transport = new TcpTransport(pollManager.poll_set);
            if (transport.connect(serv_host, serv_port))
            {
                Connection connection = new Connection();
                connectionManager.AddConnection(connection);
                ServiceServerLink<S> client = new ServiceServerLink<S>(service, persistent, request_md5sum, response_md5sum, header_values);
                lock (serviceServerLinksMutex)
                    serviceServerLinks.Add(client);
                connection.initialize(transport, false, null);
                client.initialize(connection);
                return client;
            }
            return null;
        }


        internal ServiceServerLink<M, T> CreateServiceServerLink<M, T>(string service, bool persistent, string request_md5sum,
                                                                       string response_md5sum, IDictionary<string, string> header_values)
            where M : RosMessage, new()
            where T : RosMessage, new()
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                    return null;
            }

            int serv_port = -1;
            string serv_host = "";
            if (!LookupService(service, ref serv_host, ref serv_port))
                return null;
            TcpTransport transport = new TcpTransport(pollManager.poll_set);
            if (transport.connect(serv_host, serv_port))
            {
                Connection connection = new Connection();
                connectionManager.AddConnection(connection);
                ServiceServerLink<M, T> client = new ServiceServerLink<M, T>(service, persistent, request_md5sum, response_md5sum, header_values);
                lock (serviceServerLinksMutex)
                    serviceServerLinks.Add(client);
                connection.initialize(transport, false, null);
                client.initialize(connection);
                return client;
            }
            return null;
        }


        internal void RemoveServiceServerLink<M, T>(ServiceServerLink<M, T> issl)
            where M : RosMessage, new()
            where T : RosMessage, new()
        {
            RemoveServiceServerLink((IServiceServerLink) issl);
        }


        internal void RemoveServiceServerLink<S>(ServiceServerLink<S> issl)
            where S : RosService, new()
        {
            RemoveServiceServerLink((IServiceServerLink) issl);
        }


        internal void RemoveServiceServerLink(IServiceServerLink issl)
        {
            if (shuttingDown)
                return;
            lock (serviceServerLinksMutex)
            {
                if (serviceServerLinks.Contains(issl))
                    serviceServerLinks.Remove(issl);
            }
        }


        internal bool AdvertiseService<MReq, MRes>(AdvertiseServiceOptions<MReq, MRes> ops) where MReq : RosMessage, new() where MRes : RosMessage, new()
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                    return false;
            }
            lock (servicePublicationsMutex)
            {
                if (IsServiceAdvertised(ops.service))
                {
//                    Logger.LogWarning("Tried to advertise  a service that is already advertised in this node [{0}]", ops.service);
                    return false;
                }
                if (ops.helper == null)
                    ops.helper = new ServiceCallbackHelper<MReq, MRes>(ops.srv_func);
                ServicePublication<MReq, MRes> pub = new ServicePublication<MReq, MRes>(ops.service, ops.md5sum, ops.datatype, ops.req_datatype, ops.res_datatype, ops.helper, ops.callback_queue, ops.tracked_object);
                servicePublications.Add(pub);
            }

            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, ThisNode.Name);
            args.Set(1, ops.service);
            args.Set(2, string.Format("rosrpc://{0}:{1}", Network.host, connectionManager.TCPPort));
            args.Set(3, xmlrpcManager.Uri);
            if (!Master.execute("registerService", args, result, payload, true))
            {
                throw new RosException("RPC \"registerService\" for service " + ops.service + " failed.");
            }
            return true;
        }


        internal bool UnadvertiseService(string service)
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                    return false;
            }
            IServicePublication pub = null;
            lock (servicePublicationsMutex)
            {
                foreach (IServicePublication sp in servicePublications)
                {
                    if (sp.name == service && !sp.isDropped)
                    {
                        pub = sp;
                        servicePublications.Remove(sp);
                        break;
                    }
                }
            }
            if (pub != null)
            {
                UnregisterService(pub.name);
                pub.drop();
                return true;
            }
            return false;
        }


        internal void Shutdown()
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                    return;
            }
            shuttingDown = true;
            lock (servicePublicationsMutex)
            {
                foreach (IServicePublication sp in servicePublications)
                {
                    UnregisterService(sp.name);
                    sp.drop();
                }
                servicePublications.Clear();
            }
            List<IServiceServerLink> local_service_clients;
            lock (serviceServerLinks)
            {
                local_service_clients = new List<IServiceServerLink>(serviceServerLinks);
                serviceServerLinks.Clear();
            }
            foreach (IServiceServerLink issl in local_service_clients)
            {
                issl.connection.drop(Connection.DropReason.Destructing);
            }
            local_service_clients.Clear();
        }


        internal bool LookupService(string name, ref string serv_host, ref int serv_port)
        {
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, ThisNode.Name);
            args.Set(1, name);
            if (!Master.execute("lookupService", args, result, payload, false))
            {
//                Logger.LogWarning("Service [{0}]: Not available at ROS master", name);
                return false;
            }
            string serv_uri = payload.GetString();
            if (serv_uri.Length == 0)
            {
//                Logger.LogError("Service [{0}]: Empty server URI returned from master", name);
                return false;
            }
            if (!Network.SplitUri(serv_uri, out serv_host, out serv_port))
            {
//                Logger.LogError("Service [{0}]: Bad service uri [{0}]", name, serv_uri);
                return false;
            }
            return true;
        }


        internal bool LookUpService(string mapped_name, string host, int port)
        {
            return LookupService(mapped_name, ref host, ref port);
        }


        internal bool LookUpService(string mapped_name, ref string host, ref int port)
        {
            return LookupService(mapped_name, ref host, ref port);
        }


        private bool IsServiceAdvertised(string serv_name)
        {
            List<IServicePublication> sp = new List<IServicePublication>(servicePublications);
            return sp.Any(s => s.name == serv_name && !s.isDropped);
        }


        private bool UnregisterService(string service)
        {
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, ThisNode.Name);
            args.Set(1, service);
            args.Set(2, string.Format("rosrpc://{0}:{1}", Network.host, connectionManager.TCPPort));

            bool unregisterSuccess = false;
            try
            {
                unregisterSuccess = Master.execute("unregisterService", args, result, payload, false);
            }
            catch
            {
                // ignore exception during unregister
            }
            return unregisterSuccess;
        }
    }
}
