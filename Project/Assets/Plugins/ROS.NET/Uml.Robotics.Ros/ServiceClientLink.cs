using System;
using System.Collections.Generic;
//using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Uml.Robotics.Ros
{
    public class IServiceClientLink
    {
        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<IServiceClientLink>();
        public Connection connection;
        public IServicePublication parent;
        public bool persistent;

        public bool initialize(Connection conn)
        {
            connection = conn;
            connection.DroppedEvent += onConnectionDropped;
            return true;
        }

        public bool handleHeader(Header header)
        {
            if (!header.Values.ContainsKey("md5sum") || !header.Values.ContainsKey("service") || !header.Values.ContainsKey("callerid"))
            {
                string bbq = "Error in TcpRos header. Required elements (md5sum, service, callerid) are missing";
                ROS.Error()(bbq);
                Logger.LogError(bbq);
                connection.sendHeaderError(ref bbq);
                return false;
            }
            string md5sum = (string) header.Values["md5sum"];
            string service = (string) header.Values["service"];
            string client_callerid = (string) header.Values["callerid"];

            if (header.Values.ContainsKey("persistent") && ((string) header.Values["persistent"] == "1" || (string) header.Values["persistent"] == "true"))
                persistent = true;

            //ROS.Debug()("Service client [{0}] wants service [{1}] with md5sum [{2}]", client_callerid, service, md5sum);
            Logger.LogDebug($"Service client [{client_callerid}] wants service [{service}] with md5sum [{md5sum}]" );
            IServicePublication isp = ServiceManager.Instance.LookupServicePublication(service);
            if (isp == null)
            {
                string bbq = string.Format("Received a TcpRos connection for a nonexistent service [{0}]", service);
                //ROS.Error()(bbq);
                Logger.LogWarning(bbq);
                connection.sendHeaderError(ref bbq);
                return false;
            }

            if (isp.md5sum != md5sum && md5sum != "*" && isp.md5sum != "*")
            {
                string bbq = "Client wants service " + service + " to have md5sum " + md5sum + " but it has " + isp.md5sum + ". Dropping connection";
                //ROS.Error()(bbq);
                Logger.LogError(bbq);
                connection.sendHeaderError(ref bbq);
                return false;
            }

            if (isp.isDropped)
            {
                string bbq = "[ERROR] Received a TcpRos connection for a nonexistent service [" + service + "]";
                //ROS.Error()(bbq);
                Logger.LogWarning(bbq);
                connection.sendHeaderError(ref bbq);
                return false;
            }

            parent = isp;
            IDictionary<string, string> m = new Dictionary<string, string>();
            m["request_type"] = isp.req_datatype;
            m["response_type"] = isp.res_datatype;
            m["type"] = isp.datatype;
            m["md5sum"] = isp.md5sum;
            m["callerid"] = ThisNode.Name;

            connection.writeHeader(m, onHeaderWritten);

            isp.addServiceClientLink(this);
            return true;
        }

        public virtual void processResponse(string error, bool success)
        {
            var msg = new Messages.std_msgs.String(error);
            msg.Serialized = msg.Serialize();
            byte[] buf;
            if (success)
            {
                buf = new byte[msg.Serialized.Length + 1 + 4];
                buf[0] = (byte) (success ? 0x01 : 0x00);
                msg.Serialized.CopyTo(buf, 5);
                Array.Copy(BitConverter.GetBytes(msg.Serialized.Length),0, buf, 1,4);
            }
            else
            {
                buf = new byte[1 + 4];
                buf[0] = (byte) (success ? 0x01 : 0x00);
                Array.Copy(BitConverter.GetBytes(0),0, buf, 1,4);
            }
            connection.write(buf, buf.Length, onResponseWritten);
        }

        public virtual void processResponse(RosMessage msg, bool success)
        {
            byte[] buf;
            if (success)
            {
                msg.Serialized = msg.Serialize();
                buf = new byte[msg.Serialized.Length + 1 + 4];
                buf[0] = (byte) (success ? 0x01 : 0x00);
                msg.Serialized.CopyTo(buf, 5);
                Array.Copy(BitConverter.GetBytes(msg.Serialized.Length),0, buf, 1,4);
            }
            else
            {
                buf = new byte[1 + 4];
                buf[0] = (byte) (success ? 0x01 : 0x00);
                Array.Copy(BitConverter.GetBytes(0),0, buf, 1,4);
            }
            connection.write(buf, buf.Length, onResponseWritten);
        }

        public virtual void drop()
        {
            if (connection.sendingHeaderError)
                connection.DroppedEvent -= onConnectionDropped;
            else
                connection.drop(Connection.DropReason.Destructing);
        }

        private void onConnectionDropped(Connection conn, Connection.DropReason reason)
        {
            if (conn != connection || parent == null)
                return;
            lock (parent)
            {
                parent.removeServiceClientLink(this);
            }
        }

        public virtual bool onRequestLength(Connection conn, byte[] buffer, int size, bool success)
        {
            if (!success)
                return false;

            if (conn != connection || size != 4)
                throw new Exception("Invalid request length read");

            int len = BitConverter.ToInt32(buffer, 0);
            int lengthLimit = 1000000000;
            if (len > lengthLimit)
            {
                ROS.Error()($"Message length exceeds limit of {lengthLimit}. Dropping connection.");
                connection.drop(Connection.DropReason.Destructing);
                return false;
            }
            connection.read(len, onRequest);
            return true;
        }

        public virtual bool onRequest(Connection conn, byte[] buffer, int size, bool success)
        {
            if (!success)
                return false;

            if (conn != connection)
                throw new ArgumentException("Unkown connection", nameof(conn));

            if (parent != null)
            {
                lock (parent)
                {
                    parent.processRequest(ref buffer, size, this);
                    return true;
                }
            }
            return false;
        }

        public virtual bool onHeaderWritten(Connection conn)
        {
            connection.read(4, onRequestLength);
            return true;
        }

        public virtual bool onResponseWritten(Connection conn)
        {
            if (conn != connection)
                throw new ArgumentException("Unkown connection", nameof(conn));

            if (persistent)
                connection.read(4, onRequestLength);
            else
                connection.drop(Connection.DropReason.Destructing);
            return true;
        }
    }
}
