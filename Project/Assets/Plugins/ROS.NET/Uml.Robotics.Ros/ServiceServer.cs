using System;
using System.Diagnostics;

namespace Uml.Robotics.Ros
{
    public class ServiceServer
    {
        internal NodeHandle nodeHandle;
        internal string service = "";
        internal bool unadvertised;

        public ServiceServer(string service, NodeHandle nodeHandle)
        {
            this.service = service;
            this.nodeHandle = nodeHandle;
        }

        public bool IsValid
        {
            get { return !unadvertised; }
        }

        public void shutdown()
        {
            unadvertise();
        }

        public string getService()
        {
            return service;
        }

        internal void unadvertise()
        {
            if (!unadvertised)
            {
                unadvertised = true;
                ServiceManager.Instance.UnadvertiseService(service);
            }
        }
    }
}
