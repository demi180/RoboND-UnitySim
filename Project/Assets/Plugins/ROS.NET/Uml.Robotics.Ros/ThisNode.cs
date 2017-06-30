using System;
using System.Collections.Generic;
using System.Diagnostics;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public static class ThisNode
    {
//        private static ILogger Logger { get; } = ApplicationLogging.CreateLogger(nameof(ThisNode));
        public static string Name = "empty";
        public static string Namespace = "";

        public static void Init(string n, IDictionary<string, string> remappings)
        {
            Init(n, remappings, 0);
        }

        public static void Init(string name, IDictionary<string, string> remappings, int options)
        {
            Name = name;

            bool disableAnonymous = false;
            if (remappings.ContainsKey("__name"))
            {
                Name = remappings["__name"];
                disableAnonymous = true;
            }
            if (remappings.ContainsKey("__ns"))
            {
                Namespace = remappings["__ns"];
            }
            if (string.IsNullOrEmpty(Namespace))
            {
                Namespace = "/";
            }

            long walltime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime).Ticks;
            Names.Init(remappings);
            if (Name.Contains("/"))
                throw new ArgumentException("Slashes '/' are not allowed in names", nameof(name));
            if (Name.Contains("~"))
                throw new ArgumentException("Tildes '~' are not allowed in names", nameof(name));
            try
            {
                Name = Names.Resolve(Namespace, Name);
            }
            catch (Exception e)
            {
//                Logger.LogError(e.ToString());
            }
            if ((options & (int) InitOption.AnonymousName) == (int) InitOption.AnonymousName && !disableAnonymous)
            {
                int lbefore = Name.Length;
                Name += "_" + walltime;
                if (Name.Length - lbefore > 201)
                    Name = Name.Remove(lbefore + 201);
            }
        }
    }
}
