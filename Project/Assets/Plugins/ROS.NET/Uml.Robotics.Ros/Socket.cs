using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using n = System.Net;
using ns = System.Net.Sockets;
using ROS_Comm.APMWorkaround;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public static class SocketExtensions
    {
        public static void SetTcpKeepAlive(this Socket socket, uint keepaliveTime, uint keepaliveInterval)
        {
            // Argument structure for SIO_KEEPALIVE_VALS 
            // struct tcp_keepalive
            // {
            //     u_long onoff;
            //     u_long keepalivetime;
            //     u_long keepaliveinterval;
            // };

            // marshal the equivalent of the native structure into a byte array
            byte[] inOptionValues = new byte[sizeof(UInt32) * 3];
            BitConverter.GetBytes((uint)(keepaliveTime)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)keepaliveTime).CopyTo(inOptionValues, sizeof(UInt32));
            BitConverter.GetBytes((uint)keepaliveInterval).CopyTo(inOptionValues, sizeof(UInt32) * 2);

            // write SIO_VALS to Socket IOControl
            socket.IOControl(ns.IOControlCode.KeepAliveValues, inOptionValues, null);
        }
    }

    public class Socket : IDisposable
    {
        public class SocketInfo
        {
            public int Events;
            public PollSet.SocketUpdateFunc Func;
            public int REvents;
            public TcpTransport Transport;
        }


        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<Socket>();
        internal ns.Socket realSocket { get; private set; }

        private string attemptedConnectionEndpoint;
        private bool disposed;

        public Socket(ns.Socket sock)
        {
            realSocket = sock;
            disposed = false;
        }

        public Socket(ns.AddressFamily addressFamily, ns.SocketType socketType, ns.ProtocolType protocolType)
            : this(new ns.Socket(addressFamily, socketType, protocolType))
        {
            //Logger.LogDebug("Making socket w/ FD=" + FD);
        }

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public IAsyncResult BeginConnect(n.EndPoint endpoint, AsyncCallback callback, object state)
        {
            n.IPEndPoint ipep = endpoint as n.IPEndPoint;
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            attemptedConnectionEndpoint = ipep.Address.ToString();
            return realSocket.BeginConnect(endpoint, callback, state);
        }

        public bool AcceptAsync(ns.SocketAsyncEventArgs a)
        {
            return realSocket.AcceptAsync(a);
        }

        public void Bind(n.EndPoint ep)
        {
            realSocket.Bind(ep);
        }

        public bool Blocking
        {
            get { return realSocket.Blocking; }
            set { realSocket.Blocking = value; }
        }

        public void Close()
        {
            if (realSocket != null)
                realSocket.Dispose();
        }

        public void Close(int timeout)
        {
            if (realSocket != null)
            {
                realSocket.Shutdown(ns.SocketShutdown.Send);
                realSocket.SetSocketOption(ns.SocketOptionLevel.Socket, ns.SocketOptionName.ReceiveTimeout, timeout);
                ns.SocketError unused;
                realSocket.Receive(null, 0, 0, ns.SocketFlags.None, out unused);
                realSocket.Dispose();
            }
        }

        public bool Connected
        {
            get { return realSocket != null && realSocket.Connected; }
        }

        public void EndConnect(IAsyncResult iar)
        {
            realSocket.EndConnect(iar);
        }

        public object GetSocketOption(ns.SocketOptionLevel lvl, ns.SocketOptionName n)
        {
            return realSocket.GetSocketOption(lvl, n);
        }

        public void GetSocketOption(ns.SocketOptionLevel lvl, ns.SocketOptionName n, byte[] optionvalue)
        {
            realSocket.GetSocketOption(lvl, n, optionvalue);
        }

        public byte[] GetSocketOption(ns.SocketOptionLevel lvl, ns.SocketOptionName n, int optionlength)
        {
            return realSocket.GetSocketOption(lvl, n, optionlength);
        }

        public int IOControl(int code, byte[] inval, byte[] outval)
        {
            return realSocket.IOControl(code, inval, outval);
        }

        public int IOControl(ns.IOControlCode code, byte[] inval, byte[] outval)
        {
            return realSocket.IOControl(code, inval, outval);
        }

        public n.EndPoint LocalEndPoint
        {
            get { return realSocket.LocalEndPoint; }
        }

        public void Listen(int backlog)
        {
            realSocket.Listen(backlog);
        }

        public bool NoDelay
        {
            get { return realSocket.NoDelay; }
            set { realSocket.NoDelay = value; }
        }

        public int Send(byte[] arr, int offset, int size, ns.SocketFlags f, out ns.SocketError er)
        {
            return realSocket.Send(arr, offset, size, f, out er);
        }

        public void SetSocketOption(ns.SocketOptionLevel lvl, ns.SocketOptionName n, object optionvalue)
        {
            realSocket.SetSocketOption(lvl, n, optionvalue);
        }

        public void Shutdown(ns.SocketShutdown sd)
        {
            realSocket.Shutdown(sd);
        }

        public bool SafePoll(int timeout, ns.SelectMode sm)
        {
            bool res = false;
            try
            {
                if (!disposed)
                    res = realSocket.Poll(timeout, sm);
            }
            catch (ns.SocketException e)
            {
                Logger.LogError(e.ToString());
                res = !disposed && sm == ns.SelectMode.SelectError;
            }
            return res;
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(attemptedConnectionEndpoint))
            {
                if (!realSocket.Connected)
                    attemptedConnectionEndpoint = "";
                else if (realSocket.RemoteEndPoint != null)
                {
                    n.IPEndPoint ipep = realSocket.RemoteEndPoint as n.IPEndPoint;
                    if (ipep != null)
                        attemptedConnectionEndpoint = "" + ipep.Address + ":" + ipep.Port;
                }
            }
            return " -- " + attemptedConnectionEndpoint + (Info != null ? " for " + Info.Transport._topic : "");
        }


        public const int POLLERR = 0x008;
        public const int POLLHUP = 0x010;
        public const int POLLNVAL = 0x020;
        public const int POLLIN = 0x001;
        public const int POLLOUT = 0x004;
        private int poll_timeout = 10;

        internal void _poll(int POLLFLAGS)
        {
            if (realSocket == null || !realSocket.Connected || disposed)
            {
                Info.REvents |= POLLHUP;
            }
            else
            {
                Info.REvents |= POLLFLAGS;
            }
            if (Info.REvents == 0)
            {
                return;
            }
            if (Info.Func != null &&
                ((Info.Events & Info.REvents) != 0 || (Info.REvents & POLLERR) != 0 || (Info.REvents & POLLHUP) != 0 ||
                    (Info.REvents & POLLNVAL) != 0))
            {
                bool skip = false;
                if ((Info.REvents & (POLLERR | POLLHUP | POLLNVAL)) != 0)
                {
                    if (realSocket == null || disposed || !realSocket.Connected)
                        skip = true;
                }

                if (!skip)
                {
                    Info.Func(Info.REvents & (Info.Events | POLLERR | POLLHUP | POLLNVAL));
                }
            }
            Info.REvents = 0;
        }

        internal void _poll()
        {
            int revents = 0;
            if (!realSocket.Connected || disposed)
            {
                revents |= POLLHUP;
            }
            else
            {
                if (SafePoll(poll_timeout, ns.SelectMode.SelectError))
                    revents |= POLLERR;
                if (SafePoll(poll_timeout, ns.SelectMode.SelectWrite))
                    revents |= POLLOUT;
                if (SafePoll(poll_timeout, ns.SelectMode.SelectRead))
                    revents |= POLLIN;
            }
            _poll(revents);
        }

        public SocketInfo Info { get; set; }

        public void Dispose()
        {
            disposed = true;
            if (realSocket != null)
            {
                realSocket.Dispose();
                realSocket = null;
            }
        }
    }
}