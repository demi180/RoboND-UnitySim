using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Uml.Robotics.Ros
{
    public class ServiceTypeRegistry
         : TypeRegistryBase
    {
        private static Lazy<ServiceTypeRegistry> defaultInstance = new Lazy<ServiceTypeRegistry>(LazyThreadSafetyMode.ExecutionAndPublication);

        public static ServiceTypeRegistry Default
        {
            get { return defaultInstance.Value; }
        }

        public ServiceTypeRegistry()
            : base(ApplicationLogging.CreateLogger<ServiceTypeRegistry>())
        {
        }

        public RosService CreateService(string rosServiceType)
        {
            return base.Create<RosService>(rosServiceType);
        }

        public void ParseAssemblyAndRegisterRosServices(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var typeInfo = type.GetTypeInfo();
                if (type == typeof(RosService) || !typeInfo.IsSubclassOf(typeof(RosService)))
                {
                    continue;
                }

                RosService service = Activator.CreateInstance(type) as RosService;
                if (service.ServiceType == "xamla/unkown")
                {
                    throw new Exception("Invalid servive type. Service type field (srvtype) was not initialized correctly.");
                }

                var packageName = service.ServiceType.Split('/')[0];
                if (!PackageNames.Contains(packageName))
                {
                    PackageNames.Add(packageName);
                }

                Logger.LogDebug($"Register {service.ServiceType}");
                if (!TypeRegistry.ContainsKey(service.ServiceType))
                {
                    TypeRegistry.Add(service.ServiceType, service.GetType());
                }
            }
        }

        public void Reset()
        {
            defaultInstance = new Lazy<ServiceTypeRegistry>(LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
