using System;

namespace Uml.Robotics.Ros
{
    public class PublisherLink
    {
        public class Stats
        {
            public long bytesReceived;
            public long drops;
            public long messagesReceived;
        }

        public string CallerID = "";
        public uint ConnectionID;
        public bool Latched;
        public readonly string XmlRpcUri = "";
        private Header header;
        public string md5sum = "";
        public readonly Subscription parent;
        public Stats stats = new Stats();

        public PublisherLink(Subscription parent, string xmlrpc_uri)
        {
            this.parent = parent;
            XmlRpcUri = xmlrpc_uri;
        }

        public virtual string TransportType
        {
            get { return "TCPROS"; }
        }

        public Header getHeader()
        {
            return header;
        }

        public bool setHeader(Header h)
        {
            CallerID = (string) h.Values["callerid"];
            if (!h.Values.ContainsKey("md5sum"))
                return false;
            md5sum = (string) h.Values["md5sum"];
            Latched = false;
            if (!h.Values.ContainsKey("latching"))
                return false;
            if ((string) h.Values["latching"] == "1")
                Latched = true;
            ConnectionID = ConnectionManager.Instance.GetNewConnectionId();
            header = h;
            parent.headerReceived(this, header);
            return true;
        }

        internal virtual void handleMessage(byte[] serializedmessagekinda, bool ser, bool nocopy)
        {
            throw new NotImplementedException();
        }

        public virtual void drop()
        {
            throw new NotImplementedException();
        }
    }
}
