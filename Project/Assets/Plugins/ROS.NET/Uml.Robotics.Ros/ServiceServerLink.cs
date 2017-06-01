using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class IServiceServerLink : IDisposable
    {
        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<IServiceServerLink>();
        public bool IsValid;
        public string RequestMd5Sum;
        public string RequestType;
        public string ResponseMd5Sum;
        public string ResponseType;

        private Queue<CallInfo> call_queue = new Queue<CallInfo>();
        private object call_queue_mutex = new object();
        public Connection connection;
        private CallInfo current_call;
        public bool header_read;
        private IDictionary<string, string> header_values;
        public bool header_written;
        public string name;
        public bool persistent;

        public IServiceServerLink(string name, bool persistent, string requestMd5Sum, string responseMd5Sum,
            IDictionary<string, string> header_values)
        {
            this.name = name;
            this.persistent = persistent;
            RequestMd5Sum = requestMd5Sum;
            ResponseMd5Sum = responseMd5Sum;
            this.header_values = header_values;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (connection != null && !connection.dropped)
            {
                connection.drop(Connection.DropReason.Destructing);
                connection = null;
            }
        }

        public void initialize<MSrv>()
            where MSrv : RosService, new()
        {
            MSrv srv = new MSrv();
            RequestMd5Sum = srv.RequestMessage.MD5Sum();
            ResponseMd5Sum = srv.ResponseMessage.MD5Sum();
            RequestType = srv.RequestMessage.MessageType;
            ResponseType = srv.ResponseMessage.MessageType;
        }

        public void initialize<MReq, MRes>() where MReq : RosMessage, new() where MRes : RosMessage, new()
        {
            MReq req = new MReq();
            MRes res = new MRes();
            RequestMd5Sum = req.MD5Sum();
            ResponseMd5Sum = res.MD5Sum();
            RequestType = req.MessageType;
            ResponseType = res.MessageType;
        }

        internal void initialize(Connection connection)
        {
            this.connection = connection;
            connection.DroppedEvent += onConnectionDropped;
            connection.setHeaderReceivedCallback(onHeaderReceived);

            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["service"] = name;
            dict["md5sum"] = RosService.Generate(RequestType.Replace("__Request", "").Replace("__Response", "")).MD5Sum();
            dict["callerid"] = ThisNode.Name;
            dict["persistent"] = persistent ? "1" : "0";
            if (header_values != null)
            {
                foreach (string o in header_values.Keys)
                {
                    dict[o] = header_values[o];
                }
            }
            connection.writeHeader(dict, onHeaderWritten);
        }

        private void onConnectionDropped(Connection connection, Connection.DropReason dr)
        {
            if (connection != this.connection) throw new ArgumentException("Unkown connection", nameof(connection));

            Logger.LogDebug("Service client from [{0}] for [{1}] dropped", connection.RemoteString, name);

            clearCalls();

            ServiceManager.Instance.RemoveServiceServerLink(this);

            IsValid = false;
        }

        private bool onHeaderReceived(Connection conn, Header header)
        {
            string md5sum;
            if (header.Values.ContainsKey("md5sum"))
                md5sum = (string) header.Values["md5sum"];
            else
            {
                ROS.Error()("TcpRos header from service server did not have required element: md5sum");
                Logger.LogError("TcpRos header from service server did not have required element: md5sum");
                return false;
            }
            //TODO check md5sum

            bool empty = false;
            lock (call_queue_mutex)
            {
                empty = call_queue.Count == 0;
                if (empty)
                    header_read = true;
            }

            IsValid = true;

            if (!empty)
            {
                processNextCall();
                header_read = true;
            }

            return true;
        }

        private void callFinished()
        {
            CallInfo saved_call;
            IServiceServerLink self;
            lock (call_queue_mutex)
            {
                lock (current_call.finished_mutex)
                {
                    current_call.finished = true;
                    current_call.notify_all();

                    saved_call = current_call;
                    current_call = null;

                    self = this;
                }
            }
            saved_call = new CallInfo();
            processNextCall();
        }

        private void processNextCall()
        {
            bool empty = false;
            lock (call_queue_mutex)
            {
                if (current_call != null)
                    return;
                if (call_queue.Count > 0)
                {
                    current_call = call_queue.Dequeue();
                }
                else
                    empty = true;
            }
            if (empty)
            {
                if (!persistent)
                {
                    connection.drop(Connection.DropReason.Destructing);
                }
            }
            else
            {
                RosMessage request;
                lock (call_queue_mutex)
                {
                    request = current_call.req;
                }

                request.Serialized = request.Serialize();
                byte[] tosend = new byte[request.Serialized.Length + 4];
                Array.Copy(BitConverter.GetBytes(request.Serialized.Length), tosend, 4);
                Array.Copy(request.Serialized, 0, tosend, 4, request.Serialized.Length);
                connection.write(tosend, tosend.Length, onRequestWritten);
            }
        }

        private void clearCalls()
        {
            CallInfo local_current;
            lock (call_queue_mutex)
                local_current = current_call;
            if (local_current != null)
                cancelCall(local_current);
            lock (call_queue_mutex)
            {
                while (call_queue.Count > 0)
                    cancelCall(call_queue.Dequeue());
            }
        }

        private void cancelCall(CallInfo info)
        {
            CallInfo local = info;
            lock (local.finished_mutex)
            {
                local.finished = true;
                local.notify_all();
            }
            //yield
        }

        private bool onHeaderWritten(Connection conn)
        {
            header_written = true;
            return true;
        }

        private bool onRequestWritten(Connection conn)
        {
            Logger.LogInformation("onRequestWritten(Connection conn)");
            connection.read(5, onResponseOkAndLength);
            return true;
        }

        private bool onResponseOkAndLength(Connection conn, byte[] buf, int size, bool success)
        {
            if (conn != connection)
            {
                throw new ArgumentException("Unkown connection", nameof(conn));
            }
            if (size != 5)
            {
                throw new ArgumentException($"Wrong size {size}", nameof(size));
            }
            if (!success)
                return false;

            byte ok = buf[0];
            int len = BitConverter.ToInt32(buf, 1);
            int lengthLimit = 1000000000;
            if (len > lengthLimit)
            {
                ROS.Error()($"Message length exceeds limit of {lengthLimit}. Dropping connection.");
                Logger.LogError($"Message length exceeds limit of {lengthLimit}. Dropping connection.");
                connection.drop(Connection.DropReason.Destructing);
                return false;
            }
            lock (call_queue_mutex)
            {
                if (ok != 0)
                    current_call.success = true;
                else
                    current_call.success = false;
            }
            if (len > 0)
            {
                Logger.LogDebug($"Reading message with length of {len}.");
                connection.read(len, onResponse);
            }
            else
            {
                byte[] f = new byte[0];
                onResponse(conn, f, 0, true);
            }
            return true;
        }

        private bool onResponse(Connection conn, byte[] buf, int size, bool success)
        {
            if (conn != connection)
                throw new ArgumentException("Unkown connection", nameof(conn));

            if (!success)
                return false;

            lock (call_queue_mutex)
            {
                if (current_call.success)
                {
                    if (current_call.resp == null)
                        throw new NullReferenceException("Service response is null");
                    current_call.resp.Serialized = buf;
                }
                else if (buf.Length > 0)
                {
                    // call failed with reason
                    current_call.exception = Encoding.UTF8.GetString(buf);
                }
                else { } // call failed, but no reason is given

            }

            callFinished();
            return true;
        }

        public bool call(RosService srv)
        {
            return call(srv.RequestMessage, ref srv.ResponseMessage);
        }

        public bool call(RosMessage req, ref RosMessage resp)
        {
            if (resp == null)
            {
                //instantiate null response IN CASE this call succeeds
                resp = RosMessage.Generate(req.MessageType.Replace("Request", "Response"));
            }

            CallInfo info = new CallInfo {req = req, resp = resp, success = false, finished = false};

            bool immediate = false;
            lock (call_queue_mutex)
            {
                if (connection.dropped)
                {
                    return false;
                }
                if (call_queue.Count == 0 && header_written && header_read)
                    immediate = true;
                call_queue.Enqueue(info);
            }

            if (immediate)
                processNextCall();

            while (!info.finished)
            {
                Logger.LogDebug("info.finished_condition.WaitOne();");
                info.finished_condition.WaitOne();
            }

            if (info.success)
            {
                // response is only sent on success => don't try to deserialize on failure.
                resp.Deserialize(resp.Serialized);
            }

            if (!string.IsNullOrEmpty(info.exception))
            {
                ROS.Error()("Service call failed: service [{0}] responded with an error: {1}", name, info.exception);
                Logger.LogError("Service call failed: service [{0}] responded with an error: {1}", name, info.exception);
            }
            return info.success;
        }
    }

    public class ServiceServerLink<MReq, MRes> : IServiceServerLink
        where MReq : RosMessage, new()
        where MRes : RosMessage, new()
    {
        public ServiceServerLink(string name, bool persistent, string requestMd5Sum, string responseMd5Sum,
            IDictionary<string, string> header_values)
            : base(name, persistent, requestMd5Sum, responseMd5Sum, header_values)
        {
            initialize<MReq, MRes>();
        }

        public bool call(MReq req, ref MRes resp)
        {
            RosMessage iresp = resp;
            bool r = call(req, ref iresp);
            if (iresp != null)
                resp = (MRes) iresp;
            return r;
        }
    }

    public class ServiceServerLink<MSrv> : IServiceServerLink
        where MSrv : RosService, new()
    {
        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<ServiceServerLink<MSrv>>();
        public ServiceServerLink(string name, bool persistent, string requestMd5Sum, string responseMd5Sum,
            IDictionary<string, string> header_values)
            : base(name, persistent, requestMd5Sum, responseMd5Sum, header_values)
        {
            initialize<MSrv>();
        }

        public bool call(MSrv srv)
        {
            bool result = false;
            try
            {
                bool r = call((RosService) srv);
                if (srv.ResponseMessage != null)
                    srv.ResponseMessage.Deserialize(srv.ResponseMessage.Serialized);
                else
                    srv.ResponseMessage = null;
                result = r;
            }
            catch(Exception ex)
            {
                if (ROS.isStarted() && !ROS.shutting_down)
                {
                    throw;
                }
                Logger.LogError($"Exception occurred while calling a {name}, but ROS is not started OR is shutting down\n{ex}");
            }
            return result;
        }
    }

    internal class CallInfo
    {
        internal string exception = "";
        internal bool finished;
        internal Semaphore finished_condition = new Semaphore(0, int.MaxValue);
        internal object finished_mutex = new object();
        internal RosMessage req, resp;
        internal bool success;

        internal void notify_all()
        {
            finished_condition.Release();
        }
    }
}
