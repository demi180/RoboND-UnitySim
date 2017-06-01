using System;
using System.Threading.Tasks;
using Uml.Robotics.XmlRpc;


namespace Uml.Robotics.Ros
{
    internal class PendingConnection
    {
        public string RemoteUri { get; private set; }
        public XmlRpcClient Client { get; private set; }
        public Task<XmlRpcCallResult> Task { get; private set; }

        public PendingConnection(XmlRpcClient client, Task<XmlRpcCallResult> task, string remoteUri)
        {
            this.Client = client;
            this.Task = task;
            this.RemoteUri = remoteUri;
        }
    }
}
