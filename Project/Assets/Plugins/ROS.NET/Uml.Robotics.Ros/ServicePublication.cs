using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class ServicePublication<MReq, MRes> : IServicePublication
        where MReq : RosMessage, new()
        where MRes : RosMessage, new()
    {
        public ServiceCallbackHelper<MReq, MRes> helper;

        public ServicePublication(string name, string md5Sum, string datatype, string reqDatatype, string resDatatype, ServiceCallbackHelper<MReq, MRes> helper, ICallbackQueue callback, object trackedObject)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            this.name = name;
            this.md5sum = md5Sum;
            this.datatype = datatype;
            this.req_datatype = reqDatatype;
            this.res_datatype = resDatatype;
            this.helper = helper;
            this.callback = callback;
            this.tracked_object = trackedObject;

            if (trackedObject != null)
                has_tracked_object = true;
        }

        public override void processRequest(ref byte[] buf, int num_bytes, IServiceClientLink link)
        {
            var cb = new ServiceCallback(this, helper, buf, num_bytes, link, has_tracked_object, tracked_object);
            this.callbackId = cb.Uid;
            callback.AddCallback(cb);
        }

        internal override void addServiceClientLink(IServiceClientLink iServiceClientLink)
        {
            lock (client_links_mutex)
                client_links.Add(iServiceClientLink);
        }

        internal override void removeServiceClientLink(IServiceClientLink iServiceClientLink)
        {
            lock (client_links_mutex)
                client_links.Remove(iServiceClientLink);
        }

        public class ServiceCallback : CallbackInterface
        {
            private ILogger Logger { get; } = ApplicationLogging.CreateLogger<ServiceCallback>();
            private bool _hasTrackedObject;
            private int _numBytes;
            private object _trackedObject;
            private byte[] buffer;
            private ServicePublication<MReq, MRes> isp;
            private IServiceClientLink link;

            public ServiceCallback(ServiceCallbackHelper<MReq, MRes> _helper, byte[] buf, int num_bytes, IServiceClientLink link, bool has_tracked_object, object tracked_object)
                : this(null, _helper, buf, num_bytes, link, has_tracked_object, tracked_object)
            {
            }

            public ServiceCallback(ServicePublication<MReq, MRes> sp, ServiceCallbackHelper<MReq, MRes> _helper, byte[] buf, int num_bytes, IServiceClientLink link, bool has_tracked_object, object tracked_object)
            {
                this.isp = sp;
                if (this.isp != null && _helper != null)
                    this.isp.helper = _helper;
                this.buffer = buf;
                this._numBytes = num_bytes;
                this.link = link;
                this._hasTrackedObject = has_tracked_object;
                this._trackedObject = tracked_object;
            }

            internal override CallResult Call()
            {
                if (link.connection.dropped)
                {
                    return CallResult.Invalid;
                }

                ServiceCallbackHelperParams<MReq, MRes> parms = new ServiceCallbackHelperParams<MReq, MRes>
                {
                    request = new MReq(),
                    response = new MRes(),
                    connection_header = link.connection.header.Values
                };
                parms.request.Deserialize(buffer);

                try
                {
                    bool ok = isp.helper.call(parms);
                    link.processResponse(parms.response, ok);
                }
                catch (Exception e)
                {
                    string str = "Exception thrown while processing service call: " + e;
                    ROS.Error()(str);
                    Logger.LogError(str);
                    link.processResponse(str, false);
                    return CallResult.Invalid;
                }
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

    public abstract class IServicePublication
    {
        internal ICallbackQueue callback;
        internal List<IServiceClientLink> client_links = new List<IServiceClientLink>();
        protected object client_links_mutex = new object();
        protected long callbackId = -1;
        internal string datatype;
        internal bool has_tracked_object;
        internal bool isDropped;
        internal string md5sum;
        internal string name;
        internal string req_datatype;
        internal string res_datatype;
        internal object tracked_object;

        internal void drop()
        {
            lock (client_links_mutex)
            {
                isDropped = true;
            }
            dropAllConnections();
            if (callbackId >= 0)
            {
                callback.RemoveById(callbackId);
            }
        }

        private void dropAllConnections()
        {
            List<IServiceClientLink> links;
            lock (client_links_mutex)
            {
                links = new List<IServiceClientLink>(client_links);
                client_links.Clear();
            }

            foreach (IServiceClientLink iscl in links)
            {
                iscl.connection.drop(Connection.DropReason.Destructing);
            }
        }

        internal abstract void addServiceClientLink(IServiceClientLink iServiceClientLink);
        internal abstract void removeServiceClientLink(IServiceClientLink iServiceClientLink);
        public abstract void processRequest(ref byte[] buffer, int size, IServiceClientLink iServiceClientLink);
    }
}
