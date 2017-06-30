using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class ServiceClient<MReq, MRes> : IServiceClient where MReq : RosMessage, new() where MRes : RosMessage, new()
    {
        internal ServiceClient(string service, bool persistent, IDictionary<string, string> header_values, string md5sum)
        {
            this.service = service;
            this.persistent = persistent;
            this.header_values = header_values;
            this.md5sum = md5sum;
            linkmaker = () => ServiceManager.Instance.CreateServiceServerLink<MReq, MRes>(service, persistent, md5sum, md5sum, header_values);
            if (persistent)
            {
                server_link = linkmaker();
            }
        }

        public bool call(MReq request, ref MRes response)
        {
            string md5 = request.MD5Sum();
            return call(request, ref response, md5);
        }

        public bool call(MReq request, ref MRes response, string service_md5sum)
        {
            if (!precall(service_md5sum) || server_link == null)
            {
                shutdown();
                return false;
            }
            var serviceServerLink = server_link as ServiceServerLink<MReq, MRes>;
            return postcall(serviceServerLink != null && serviceServerLink.call(request, ref response));
        }
    }

    public class ServiceClient<MSrv> : IServiceClient
        where MSrv : RosService, new()
    {
        internal ServiceClient(string service, bool persistent, IDictionary<string, string> header_values, string md5sum)
        {
            this.service = service;
            this.persistent = persistent;
            this.header_values = header_values;
            this.md5sum = md5sum;
            linkmaker = () => ServiceManager.Instance.CreateServiceServerLink<MSrv>(service, persistent, md5sum, md5sum, header_values);
            if (persistent)
            {
                server_link = linkmaker();
            }
        }

        public bool call(MSrv srv)
        {
            string md5 = srv.RequestMessage.MD5Sum();
            return call(srv, md5);
        }

        public bool call(MSrv srv, string service_md5sum)
        {
            if (!precall(service_md5sum) || server_link == null)
            {
                shutdown();
                return false;
            }
            var serviceServerLink = server_link as ServiceServerLink<MSrv>;
            return postcall(serviceServerLink != null && serviceServerLink.call(srv));
        }
    }

    public class IServiceClient
    {
//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<IServiceClient>();

        internal IDictionary<string, string> header_values;
        internal bool is_shutdown;
        internal string md5sum;
        internal bool persistent;
        internal IServiceServerLink server_link;
        internal string service;
        protected delegate IServiceServerLink ServerLinkMakerDelegate();
        protected ServerLinkMakerDelegate linkmaker;

        protected IServiceClient()
        {
        }

        public bool IsValid
        {
            get { return !persistent || (!is_shutdown && server_link != null && server_link.IsValid); }
        }

        protected bool precall(string service_md5sum)
        {
            if (service_md5sum != md5sum)
            {
//                Logger.LogError("Call to service [{0} with md5sum [{1} does not match md5sum when the handle was created([{2}])", service, service_md5sum, md5sum);
                return false;
            }
            if (server_link != null && server_link.connection.dropped)
            {
				if ( persistent )
					UnityEngine.Debug.LogWarning ( "W1" );
//                    Logger.LogWarning("Persistent service client's server link has been dropped. Trying to reconnect to proceed with this call");
                server_link = null;
            }
            if (is_shutdown && persistent)
//                Logger.LogWarning("Persistent service client is self-resurrecting");
            is_shutdown = false;
            if (persistent && server_link == null || !persistent)
            {
                server_link = linkmaker();
            }
            return true;
        }

        protected bool postcall(bool retval)
        {
            while (ROS.shuttingDown && ROS.ok)
            {
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, ROS.WallDuration));
            }
            return retval;
        }

        public void shutdown()
        {
            if (!is_shutdown)
            {
                is_shutdown = true;
                if (!persistent && server_link != null)
                {
                    ServiceManager.Instance.RemoveServiceServerLink(server_link);
                    server_link = null;
                }
            }
        }
    }
}
