using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class TcpTransport
    {
        public delegate void AcceptCallback(TcpTransport trans);
        public delegate void DisconnectFunc(TcpTransport trans);
        public delegate void HeaderReceivedFunc(TcpTransport trans, Header header);
        public delegate void ReadFinishedFunc(TcpTransport trans);
        public delegate void WriteFinishedFunc(TcpTransport trans);

        [Flags]
        public enum Flags
        {
            SYNCHRONOUS = 1 << 0
        }

        public static bool use_keepalive = true;

//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<TcpTransport>();

        const int BITS_PER_BYTE = 8;
        const int POLLERR = 0x008;
        const int POLLHUP = 0x010;
        const int POLLNVAL = 0x020;
        const int POLLIN = 0x001;
        const int POLLOUT = 0x004;

        public IPEndPoint LocalEndPoint;
        public string _topic;
        public string connectedHost;
        public int connectedPort;
        public string cachedRemoteHost = "";

        private object closeMutex = new object();
        private bool closed;
        private bool expectingRead;
        private bool expectingWrite;
        private int flags;
        private bool isServer;
        private PollSet pollSet;
        private int serverPort = -1;

        private Socket socket;

        public TcpTransport()
        {
        }


        public TcpTransport(System.Net.Sockets.Socket s, PollSet pollset)
            : this(s, pollset, 0)
        {
        }

        public TcpTransport(System.Net.Sockets.Socket s, PollSet pollset, int flags)
            : this(pollset, flags)
        {
            setSocket(new Socket(s));
        }

        public TcpTransport(PollSet pollset)
            : this(pollset, 0)
        {
        }

        public TcpTransport(PollSet pollset, int flags)
        {
            if (pollset != null)
            {
                pollSet = pollset;
                pollSet.DisposingEvent += close;
            }
            else
            {
//                Logger.LogError("Null pollset in tcptransport ctor");
            }
            this.flags = flags;
        }

        public string ClientUri
        {
            get
            {
                if (connectedHost == null || connectedPort == 0)
                    return "[NOT CONNECTED]";
                return "http://" + connectedHost + ":" + connectedPort + "/";
            }
        }

        public string Topic
        {
            get { return _topic != null ? _topic : "?!?!?!"; }
        }

        public virtual bool getRequiresHeader()
        {
            return true;
        }

        public event AcceptCallback accept_cb;
        public event DisconnectFunc disconnect_cb;
        public event WriteFinishedFunc write_cb;
        public event ReadFinishedFunc read_cb;

        public bool setNonBlocking()
        {
            if ((flags & (int) Flags.SYNCHRONOUS) == 0)
            {
                try
                {
                    socket.Blocking = false;
                }
                catch (Exception e)
                {
//                    Logger.LogError(e.ToString());
                    close();
                    return false;
                }
            }

            return true;
        }

        public void setNoDelay(bool nd)
        {
            try
            {
                socket.NoDelay = nd;
            }
            catch (Exception e)
            {
//                Logger.LogError(e.ToString());
            }
        }

        public void enableRead()
        {
            if (socket == null)
                return;
            if (!socket.Connected)
                close();
            lock (closeMutex)
            {
                if (closed)
                    return;
            }
            if (!expectingRead && pollSet != null)
            {
                //Console.WriteLine("ENABLE READ:   " + Topic + "(" + sock.FD + ")");
                expectingRead = true;
                pollSet.AddEvents(socket, POLLIN);
            }
        }

        public void disableRead()
        {
            if (socket == null)
                return;
            if (!socket.Connected)
                close();
            lock (closeMutex)
            {
                if (closed)
                    return;
            }
            if (expectingRead && pollSet != null)
            {
                //Console.WriteLine("DISABLE READ:  " + Topic + "(" + sock.FD + ")");
                pollSet.RemoveEvents(socket, POLLIN);
                expectingRead = false;
            }
        }

        public void enableWrite()
        {
            if (socket == null)
                return;
            if (!socket.Connected) close();
            lock (closeMutex)
            {
                if (closed)
                    return;
            }
            if (!expectingWrite && pollSet != null)
            {
                //Console.WriteLine("ENABLE WRITE:  " + Topic + "(" + sock.FD + ")");
                expectingWrite = true;
                pollSet.AddEvents(socket, POLLOUT);
            }
        }

        public void disableWrite()
        {
            if (socket == null)
                return;
            if (!socket.Connected) close();
            lock (closeMutex)
            {
                if (closed)
                    return;
            }
            if (expectingWrite && pollSet != null)
            {
                //Console.WriteLine("DISABLE WRITE: " + Topic + "(" + sock.FD + ")");
                pollSet.RemoveEvents(socket, POLLOUT);
                expectingWrite = false;
            }
        }

        public bool connect(string host, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectedHost = host;
            connectedPort = port;
            if (!setNonBlocking())
                throw new Exception("Failed to make socket nonblocking");
            setNoDelay(true);

            IPAddress ip;
            if (!IPAddress.TryParse(host, out ip))
            {
                ip = Dns.GetHostAddressesAsync(host).Result.Where(x => !x.ToString().Contains(":")).FirstOrDefault();
                if (ip == null)
                {
                    close();
//                    Logger.LogError("Couldn't resolve host name [{0}]", host);
                    return false;
                }
            }

            if (ip == null)
                return false;

            IPEndPoint ipep = new IPEndPoint(ip, port);
            LocalEndPoint = ipep;
            DateTime connectionAttempted = DateTime.UtcNow;

            IAsyncResult asyncres = socket.BeginConnect(ipep, iar =>
            {
                lock (this)
                {
                    if (socket != null)
                    {
                        try
                        {
                            socket.EndConnect(iar);
                        }
                        catch (Exception e)
                        {
//                            Logger.LogError(e.ToString());
                        }
                    }
                }
            }, null);

            bool completed = false;
            while (ROS.ok && !ROS.shutting_down)
            {
                completed = asyncres.AsyncWaitHandle.WaitOne(10);
                if (completed)
                    break;
                if (DateTime.UtcNow.Subtract(connectionAttempted).TotalSeconds >= 3)
                {
//                    Logger.LogInformation("Trying to connect for " + DateTime.UtcNow.Subtract(connectionAttempted).TotalSeconds + "s\t: " + this);
                    if (!asyncres.AsyncWaitHandle.WaitOne(100))
                    {
                        socket.Close();
                        socket = null;
                    }
                }
            }

            if (!completed || socket == null || !socket.Connected)
            {
                return false;
            }
            else
            {
//                Logger.LogDebug("TcpTransport connection established.");
            }

            return ROS.ok && initializeSocket();
        }

        public bool listen(int port, int backlog, AcceptCallback accept_cb)
        {
            isServer = true;
            this.accept_cb = accept_cb;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            setNonBlocking();
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverPort = ((IPEndPoint) socket.LocalEndPoint).Port;
            socket.Listen(backlog);
            if (!initializeSocket())
                return false;
            if ((flags & (int)Flags.SYNCHRONOUS) == 0)
                enableRead();
            return true;
        }

        public void parseHeader(Header header)
        {
            if (_topic == null)
            {
                if (header.Values.ContainsKey("topic"))
                    _topic = header.Values["topic"].ToString();
            }

            if (header.Values.ContainsKey("tcp_nodelay"))
            {
                var nodelay = (string)header.Values["tcp_nodelay"];
                if (nodelay == "1")
                {
                    setNoDelay(true);
                }
            }
        }

        private bool TrySetKeepAlive(Socket sock, uint time, uint interval)
        {
            try
            {
                sock.SetTcpKeepAlive(time, interval);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void setKeepAlive(bool use, int idle, int interval, int count)
        {
            if (use)
            {
                if (!TrySetKeepAlive(socket, (uint)idle, (uint)interval))
                {
                    try
                    {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, use);
                    }
                    catch (Exception e)
                    {
//                        Logger.LogError(e.ToString());
                        return;
                    }
                }
            }
        }

        public int read(byte[] buffer, int pos, int length)
        {
            lock (closeMutex)
            {
                if (closed)
                    return -1;
            }
            int num_bytes = 0;
            SocketError err;
            num_bytes = socket.realSocket.Receive(buffer, pos, length, SocketFlags.None, out err);
            if (num_bytes < 0)
            {
                if (err == SocketError.TryAgain || err == SocketError.WouldBlock)
                    num_bytes = 0;
                else if (err != SocketError.InProgress && err != SocketError.IsConnected && err != SocketError.Success)
                {
                    close();
                    num_bytes = -1;
                }
            }
            return num_bytes;
        }

        public int write(byte[] buffer, int pos, int size)
        {
            lock (closeMutex)
            {
                if (closed)
                    return -1;
            }

            SocketError err;
            int num_bytes = socket.Send(buffer, pos, size, SocketFlags.None, out err);
            if (num_bytes <= 0)
            {
                if (err == SocketError.TryAgain || err == SocketError.WouldBlock)
                    num_bytes = 0;
                else if (err != SocketError.InProgress && err != SocketError.IsConnected && err != SocketError.Success)
                {
                    close();
                    return -1;
                }
                else
                    return 0;
            }
            return num_bytes;
        }

        private bool initializeSocket()
        {
            if (!setNonBlocking())
                return false;
            setNoDelay(true);
            setKeepAlive(use_keepalive, 60, 10, 9);

            if (string.IsNullOrEmpty(cachedRemoteHost))
            {
                if (isServer)
                    cachedRemoteHost = "TCPServer Socket";
                else
                    cachedRemoteHost = this.ClientUri + " on socket " + socket.realSocket.RemoteEndPoint.ToString();
            }

            if (pollSet != null)
            {
                pollSet.AddSocket(socket, socketUpdate, this);
            }
            if (!isServer && !socket.Connected)
            {
                close();
                return false;
            }
            return true;
        }

        private bool setSocket(Socket socket)
        {
            this.socket = socket;
            return initializeSocket();
        }

        public TcpTransport accept()
        {
            var args = new SocketAsyncEventArgs();
            if (socket == null || !socket.AcceptAsync(args))
                return null;

            if (args.AcceptSocket == null)
            {
//                Logger.LogError("Nothing to accept, return null");
                return null;
            }

            var acc = new Socket(args.AcceptSocket);
            var transport = new TcpTransport(pollSet, flags);
            if (!transport.setSocket(acc))
            {
                throw new InvalidOperationException("Could not add socket to transport");
            }
            return transport;
        }

        public override string ToString()
        {
            return "TCPROS connection to [" + socket + "]";
        }

        private void socketUpdate(int events)
        {
            lock (closeMutex)
            {
                if (closed)
                    return;
            }

            if (isServer)
            {
                TcpTransport transport = accept();
                if (transport != null)
                {
                    if (accept_cb == null)
                        throw new NullReferenceException("Accept callback is null");

                    accept_cb(transport);
                }
            }
            else
            {
                if ((events & POLLIN) != 0 && expectingRead) //POLL IN FLAG
                {
                    if (read_cb != null)
                    {
                        read_cb(this);
                    }
                }

                if ((events & POLLOUT) != 0 && expectingWrite)
                {
                    if (write_cb != null)
                        write_cb(this);
                }

                if ((events & POLLERR) != 0 || (events & POLLHUP) != 0 || (events & POLLNVAL) != 0)
                {
                    int error = 0;
                    try
                    {
                        error = (int) socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);
                    }
                    catch (Exception e)
                    {
//                        Logger.LogError("Failed to get sock options! (error: " + error + ")" + e);
                    }
					if ( error != 0 )
						UnityEngine.Debug.Log ( "socket error = " + error );
//                        Logger.LogError("Socket error = " + error);
                    close();
                }
            }
        }

        public void close()
        {
            DisconnectFunc disconnect_cb = null;
            lock (closeMutex)
            {
                if (!closed)
                {
                    closed = true;
                    if (pollSet != null)
                        pollSet.RemoveSocket(socket);
                    if (socket.Connected)
                        socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket = null;
                    disconnect_cb = this.disconnect_cb;
                    this.disconnect_cb = null;
                    read_cb = null;
                    write_cb = null;
                    accept_cb = null;
                }
            }

            if (disconnect_cb != null)
            {
                disconnect_cb(this);
            }
        }
    }
}
