//using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Uml.Robotics.XmlRpc
{
    public abstract class XmlRpcSource : IDisposable
    {
        private const int READ_BUFFER_LENGTH = 4096;

//        private ILogger Logger { get; } = XmlRpcLogging.CreateLogger<XmlRpcSource>();

        private bool keepOpen;      // In the client, keep connections open if you intend to make multiple calls.

        public bool KeepOpen
        {
            get { return keepOpen; }
            set { keepOpen = value; }
        }

        public virtual NetworkStream getStream()
        {
            return null;
        }

        public virtual Socket getSocket()
        {
            return null;
        }

        public virtual void Close()
        {
            throw new NotImplementedException();
        }

        public virtual XmlRpcDispatch.EventType HandleEvent(XmlRpcDispatch.EventType eventType)
        {
            throw new NotImplementedException();
        }

        internal virtual bool readHeader(ref HttpHeader header)
        {
            // Read available data
            int dataLen = 0;
            var stream = getStream();
            if (stream == null)
            {
                throw new Exception("Could not access network stream");
            }

            byte[] data = new byte[READ_BUFFER_LENGTH];
            try
            {
                dataLen = stream.Read(data, 0, READ_BUFFER_LENGTH);

                if (dataLen == 0)
                    return false;   // If it is disconnect

                if (header == null)
                {
                    header = new HttpHeader(Encoding.ASCII.GetString(data, 0, dataLen));
                    Debug.Assert(header.HeaderStatus != HttpHeader.ParseStatus.UNINITIALIZED);
                }
                else if (header.Append(Encoding.ASCII.GetString(data, 0, dataLen)) == HttpHeader.ParseStatus.PARTIAL_HEADER)
                    return true; //if we successfully append a piece of the header, return true, but DO NOT change states
            }
            catch (SocketException ex)
            {
//                Logger.LogError("XmlRpcServerConnection::readHeader: error while reading header ({0}).", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
//                Logger.LogError("XmlRpcServerConnection::readHeader: error while reading header ({0}).", ex.Message);
                return false;
            }

            if (header.HeaderStatus != HttpHeader.ParseStatus.COMPLETE_HEADER)
                return false;

            return true;
        }

        public void Dispose()
        {
            Close();
        }

        // In the server, a new source (XmlRpcServerConnection) is created
        // for each connected client. When each connection is closed, the
        // corresponding source object is deleted.
    }
}
