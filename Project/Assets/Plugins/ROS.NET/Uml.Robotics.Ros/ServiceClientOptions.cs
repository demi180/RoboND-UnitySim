using System.Collections.Generic;

namespace Uml.Robotics.Ros
{
    public class ServiceClientOptions
    {
        public IDictionary<string, string> header_values;
        public string md5sum;
        public bool persistent;
        public string service;

        public ServiceClientOptions(string service, bool persistent, IDictionary<string, string> header_values)
            : this(service, persistent, header_values, "")
        {
        }

        public ServiceClientOptions(string service, bool persistent, IDictionary<string, string> header_values, string md5sum)
        {
            this.header_values = header_values;
            this.md5sum = md5sum;
            this.persistent = persistent;
            this.service = service;
        }
    }
}
