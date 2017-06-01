using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Uml.Robotics.XmlRpc;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class XmlRpcManager : IDisposable
    {
        public static XmlRpcManager Instance
        {
            get { return instance.Value; }
        }

        internal static void Terminate()
        {
            XmlRpcManager.Instance.Shutdown();
        }

        internal static void Reset()
        {
            instance = new Lazy<XmlRpcManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static Lazy<XmlRpcManager> instance = new Lazy<XmlRpcManager>(LazyThreadSafetyMode.ExecutionAndPublication);

        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<XmlRpcManager>();
        private Dictionary<string, XmlRpcServerMethod> functions = new Dictionary<string, XmlRpcServerMethod>();
        private object functionsGate = new object();
        private XmlRpcFunc getPid;
        private XmlRpcServer server;
        private Thread serverThread;
        private bool shuttingDown;
        private string uri = "";
        private int port;


        public static Action<XmlRpcValue> ResponseStr(IntPtr target, int code, string msg, string response)
        {
            return (XmlRpcValue v) =>
            {
                v.Set(0, code);
                v.Set(1, msg);
                v.Set(2, response);
            };
        }


        public static Action<XmlRpcValue> ResponseInt(int code, string msg, int response)
        {
            return (XmlRpcValue v) =>
            {
                v.Set(0, code);
                v.Set(1, msg);
                v.Set(2, response);
            };
        }


        public static Action<XmlRpcValue> ResponseBool(int code, string msg, bool response)
        {
            return (XmlRpcValue v) =>
            {
                v.Set(0, code);
                v.Set(1, msg);
                v.Set(2, response);
            };
        }


        public XmlRpcManager()
        {
            this.server = new XmlRpcServer();
            this.getPid = (parms, result) => ResponseInt(1, "", Process.GetCurrentProcess().Id)(result);
        }


        public string Uri
        {
            get { return uri; }
        }


        public bool IsShuttingDown
        {
            get { return shuttingDown; }
        }


        public void Dispose()
        {
            Shutdown();
        }


        private void ServerThreadFunc()
        {
            while (!shuttingDown)
            {
                if (server.Dispatch == null)
                {
                    throw new NullReferenceException("XmlRpcManager is not initialized yet!");
                }

                lock (functionsGate)
                {
                    server.Work(TimeSpan.FromMilliseconds(1));
                }
            }
        }


        public bool ValidateXmlRpcResponse(string method, XmlRpcValue response, XmlRpcValue payload)
        {
            if (response.Type != XmlRpcType.Array)
                return ValidateFailed(method, "didn't return an array -- {0}", response);
            if (response.Count != 3)
                return ValidateFailed(method, "didn't return a 3-element array -- {0}", response);
            if (response[0].Type != XmlRpcType.Int)
                return ValidateFailed(method, "didn't return an int as the 1st element -- {0}", response);
            int status_code = response[0].GetInt();
            if (response[1].Type != XmlRpcType.String)
                return ValidateFailed(method, "didn't return a string as the 2nd element -- {0}", response);

            string status_string = response[1].GetString();
            if (status_code != 1)
            {
                return ValidateFailed(method, "returned an error ({0}): [{1}] -- {2}", status_code, status_string, response);
            }

            switch (response[2].Type)
            {
                case XmlRpcType.Array:
                    {
                        payload.SetArray(0);
                        for (int i = 0; i < response[2].Count; i++)
                        {
                            payload.Set(i, response[2][i]);
                        }
                    }
                    break;
                case XmlRpcType.Int:
                case XmlRpcType.Double:
                case XmlRpcType.String:
                case XmlRpcType.Boolean:
                    payload.Copy(response[2]);
                    break;
                case XmlRpcType.Empty:
                    break;
                default:
                    throw new ArgumentException("Unhandled valid XML-RPC payload type: " + response[2].Type, nameof(response));
            }
            return true;
        }


        private bool ValidateFailed(string method, string errorFormat, params object[] args)
        {
            Logger.LogDebug("XML-RPC Call [{0}] {1} failed validation", method, string.Format(errorFormat, args));
            return false;
        }


        public bool Bind(string functionName, XmlRpcFunc callback)
        {
            lock (functionsGate)
            {
                if (functions.ContainsKey(functionName))
                    return false;

                var method = new XmlRpcServerMethod(server, functionName, callback);
                functions.Add(functionName, method);
                server.AddMethod(method);
            }

            return true;
        }


        public void Unbind(string functionName)
        {
            lock (functionsGate)
            {
                functions.Remove(functionName);
            }
        }


        /// <summary>
        /// This function starts the XmlRpcServer used to handle inbound calls on this node
        /// </summary>
        /// <param name="port">The optional argument is used to force ROS to try to bind to a specific port.
        /// Doing so should only be done when acting as the RosMaster.</param>
        public void Start(int port = 0)
        {
            shuttingDown = false;

            Bind("getPid", getPid);

            // if port is 0, then we need to get our hostname from ROS' network init,
            // and we don't know our port until we're bound and listening
            bool bound = server.BindAndListen(port);
            if (!bound)
                throw new Exception("RPCServer bind failed");

            if (port == 0)
            {
                this.port = server.Port;     // get bind result
                this.uri = "http://" + Network.host + ":" + this.port + "/";
            }
            else
            {
                this.port = port;
                this.uri = ROS.ROS_MASTER_URI;       // if port is not 0 we are be the master
            }

            Logger.LogInformation("XmlRpc Server listening at " + uri);
            serverThread = new Thread(ServerThreadFunc) { IsBackground = true };
            serverThread.Start();
        }


        internal void Shutdown()
        {
            if (shuttingDown)
                return;

            shuttingDown = true;
            serverThread.Join();
            server.Shutdown();

            lock (functionsGate)
            {
                functions.Clear();
            }

            Logger.LogDebug("XmlRpc Server shutted down.");
        }
    }
}
