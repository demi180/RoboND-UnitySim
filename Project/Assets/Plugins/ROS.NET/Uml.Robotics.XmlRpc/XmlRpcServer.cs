using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.XmlRpc
{
    public class XmlRpcServer : XmlRpcSource
    {
        const string XMLRPC_VERSION = "XMLRPC++ 0.7";

        const string SYSTEM_MULTICALL = "system.multicall";
        const string METHODNAME = "methodName";
        const string PARAMS = "params";

        const string FAULTCODE = "faultCode";
        const string FAULTSTRING = "faultString";
        const string LIST_METHODS = "system.listMethods";
        const string METHOD_HELP = "system.methodHelp";
        const string MULTICALL = "system.multicall";

        private ILogger Logger { get; } = XmlRpcLogging.CreateLogger<XmlRpcServer>();
        private XmlRpcDispatch _disp = new XmlRpcDispatch();

        private bool _introspectionEnabled;     // whether the introspection API is supported by this server
        private XmlRpcServerMethod methodListMethods;
        private XmlRpcServerMethod methrodHelp;
        private Dictionary<string, XmlRpcServerMethod> _methods = new Dictionary<string, XmlRpcServerMethod>();
        private int _port;
        private TcpListener listener;

        public XmlRpcServer()
        {
            methodListMethods = new ListMethodsMethod(this);
            methrodHelp = new HelpMethod(this);
        }

        public void Shutdown()
        {
            _disp.Clear();
            listener.Stop();
        }

        public int Port
        {
            get { return _port; }
        }

        public XmlRpcDispatch Dispatch
        {
            get { return _disp; }
        }

        public void AddMethod(XmlRpcServerMethod method)
        {
            _methods.Add(method.Name, method);
        }

        public void RemoveMethod(XmlRpcServerMethod method)
        {
            foreach (var rec in _methods)
            {
                if (method == rec.Value)
                {
                    _methods.Remove(rec.Key);
                    break;
                }
            }
        }

        public void RemoveMethod(string name)
        {
            _methods.Remove(name);
        }

        public void Work(TimeSpan timeSlice)
        {
            _disp.Work(timeSlice);
        }

        public XmlRpcServerMethod FindMethod(string name)
        {
            if (_methods.ContainsKey(name))
                return _methods[name];
            return null;
        }

        public override Socket getSocket()
        {
            return listener?.Server;
        }

        public bool BindAndListen(int port, int backlog = 5)
        {
            try
            {
                _port = port;
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start(backlog);
                _port = ((IPEndPoint)listener.Server.LocalEndPoint).Port;
                _disp.AddSource(this, XmlRpcDispatch.EventType.ReadableEvent);

                Logger.LogInformation("XmlRpcServer::bindAndListen: server listening on port {0}", _port);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        // Handle input on the server socket by accepting the connection
        // and reading the rpc request.
        public override XmlRpcDispatch.EventType HandleEvent(XmlRpcDispatch.EventType eventType)
        {
            acceptConnection();
            return XmlRpcDispatch.EventType.ReadableEvent;  // Continue to monitor this fd
        }

        // Accept a client connection request and create a connection to
        // handle method calls from the client.
        private void acceptConnection()
        {
            while (listener.Pending())
            {
                try
                {
                    _disp.AddSource(new XmlRpcServerConnection(listener.AcceptSocketAsync().Result, this), XmlRpcDispatch.EventType.ReadableEvent);
                    Logger.LogInformation("XmlRpcServer::acceptConnection: creating a connection");
                }
                catch (SocketException ex)
                {
                    Logger.LogError("XmlRpcServer::acceptConnection: Could not accept connection ({0}).", ex.Message);
                    Thread.Sleep(10);
                }
            }
        }

        public void removeConnection(XmlRpcServerConnection sc)
        {
            _disp.RemoveSource(sc);
        }


        // Introspection support


        // Specify whether introspection is enabled or not. Default is enabled.
        public void enableIntrospection(bool enabled)
        {
            if (_introspectionEnabled == enabled)
                return;

            _introspectionEnabled = enabled;

            if (enabled)
            {
                AddMethod(methodListMethods);
                AddMethod(methrodHelp);
            }
            else
            {
                RemoveMethod(LIST_METHODS);
                RemoveMethod(METHOD_HELP);
            }
        }

        private void listMethods(XmlRpcValue result)
        {
            result.SetArray(_methods.Count + 1);

            int i = 0;
            foreach (var rec in _methods)
            {
                result.Set(i++, rec.Key);
            }

            // Multicall support is built into XmlRpcServerConnection
            result.Set(i, MULTICALL);
        }

        // Run the method, generate _response string
        public string executeRequest(string request)
        {
            string response = "";
            XmlRpcValue parms = new XmlRpcValue(), resultValue = new XmlRpcValue();
            string methodName = parseRequest(parms, request);
            Logger.LogWarning("XmlRpcServerConnection::executeRequest: server calling method '{0}'", methodName);

            try
            {
                if (!executeMethod(methodName, parms, resultValue) &&
                    !executeMulticall(methodName, parms, resultValue))
                    response = generateFaultResponse(methodName + ": unknown method name");
                else
                    response = generateResponse(resultValue.ToXml());
            }
            catch (XmlRpcException fault)
            {
                Logger.LogWarning("XmlRpcServerConnection::executeRequest: fault {0}.", fault.Message);
                response = generateFaultResponse(fault.Message, fault.ErrorCode);
            }
            return response;
        }

        // Execute a named method with the specified params.
        public bool executeMethod(string methodName, XmlRpcValue parms, XmlRpcValue result)
        {
            XmlRpcServerMethod method = FindMethod(methodName);

            if (method == null)
                return false;

            method.Execute(parms, result);

            // Ensure a valid result value
            if (!result.IsEmpty)
                result.Set("");

            return true;
        }

        // Create a response from results xml
        public string generateResponse(string resultXml)
        {
            const string RESPONSE_1 = "<?xml version=\"1.0\"?>\r\n<methodResponse><params><param>\r\n\t";
            const string RESPONSE_2 = "\r\n</param></params></methodResponse>\r\n";

            string body = RESPONSE_1 + resultXml + RESPONSE_2;
            string header = generateHeader(body);
            string result = header + body;
            Logger.LogDebug("XmlRpcServerConnection::generateResponse:\n{0}\n", result);
            return result;
        }

        // Parse the method name and the argument values from the request.
        private string parseRequest(XmlRpcValue parms, string request)
        {
            string methodName = "unknown";

            var requestDocument = XDocument.Parse(request);
            var methodCallElement = requestDocument.Element("methodCall");
            if (methodCallElement == null)
                throw new XmlRpcException("Expected <methodCall> element of XML-RPC is missing.");

            var methodNameElement = methodCallElement.Element("methodName");
            if (methodNameElement != null)
                methodName = methodNameElement.Value;

            var xmlParameters = methodCallElement.Element("params").Elements("param").ToList();

            if (xmlParameters.Count > 0)
            {
                parms.SetArray(xmlParameters.Count);

                for (int i = 0; i < xmlParameters.Count; i++)
                {
                    var value = new XmlRpcValue();
                    value.FromXElement(xmlParameters[i].Element("value"));
                    parms.Set(i, value);
                }
            }

            return methodName;
        }

        // Prepend http headers
        private string generateHeader(string body)
        {
            return string.Format(
                "HTTP/1.1 200 OK\r\n" +
                "Server: {0}\r\n" +
                "Content-Type: text/xml\r\n" +
                "Content-length: {1}\r\n\r\n",
                XMLRPC_VERSION,
                body.Length
            );
        }

        public string generateFaultResponse(string errorMsg, int errorCode = -1)
        {
            const string RESPONSE_1 = "<?xml version=\"1.0\"?>\r\n<methodResponse><fault>\r\n\t";
            const string RESPONSE_2 = "\r\n</fault></methodResponse>\r\n";

            var faultStruct = new XmlRpcValue();
            faultStruct.Set(FAULTCODE, errorCode);
            faultStruct.Set(FAULTSTRING, errorMsg);
            string body = RESPONSE_1 + faultStruct.ToXml() + RESPONSE_2;
            string header = generateHeader(body);

            return header + body;
        }

        // Execute multiple calls and return the results in an array.
        public bool executeMulticall(string methodNameRoot, XmlRpcValue parms, XmlRpcValue result)
        {
            if (methodNameRoot != SYSTEM_MULTICALL)
                return false;

            // There ought to be 1 parameter, an array of structs
            if (parms.Count != 1 || parms[0].Type != XmlRpcType.Array)
                throw new XmlRpcException(SYSTEM_MULTICALL + ": Invalid argument (expected an array)");

            int nc = parms[0].Count;
            result.SetArray(nc);

            for (int i = 0; i < nc; ++i)
            {
                if (!parms[0][i].HasMember(METHODNAME) ||
                    !parms[0][i].HasMember(PARAMS))
                {
                    result[i].Set(FAULTCODE, -1);
                    result[i].Set(FAULTSTRING, SYSTEM_MULTICALL + ": Invalid argument (expected a struct with members methodName and params)");
                    continue;
                }

                string methodName = parms[0][i][METHODNAME].GetString();
                XmlRpcValue methodParams = parms[0][i][PARAMS];

                XmlRpcValue resultValue = new XmlRpcValue();
                resultValue.SetArray(1);
                try
                {
                    if (!executeMethod(methodName, methodParams, resultValue[0]) &&
                        !executeMulticall(methodName, parms, resultValue[0]))
                    {
                        result[i].Set(FAULTCODE, -1);
                        result[i].Set(FAULTSTRING, methodName + ": unknown method name");
                    }
                    else
                    {
                        result[i] = resultValue;
                    }
                }
                catch (XmlRpcException fault)
                {
                    result[i].Set(FAULTCODE, 0);
                    result[i].Set(FAULTSTRING, fault.Message);
                }
            }

            return true;
        }

        private class ListMethodsMethod : XmlRpcServerMethod
        {
            public ListMethodsMethod(XmlRpcServer server)
                : base(server, LIST_METHODS)
            {
            }

            public override void Execute(XmlRpcValue parms, XmlRpcValue result)
            {
                this.Server.listMethods(result);
            }

            public override string Help()
            {
                return "List all methods available on a server as an array of strings";
            }
        };


        // Retrieve the help string for a named method
        private class HelpMethod : XmlRpcServerMethod
        {
            public HelpMethod(XmlRpcServer server)
                : base(server, METHOD_HELP)
            {
            }

            public override void Execute(XmlRpcValue parms, XmlRpcValue result)
            {
                if (parms[0].Type != XmlRpcType.String)
                    throw new XmlRpcException(METHOD_HELP + ": Invalid argument type");

                var method = this.Server.FindMethod(parms[0].GetString());
                if (method == null)
                    throw new XmlRpcException(METHOD_HELP + ": Unknown method name");

                result.Set(method.Help());
            }

            public override string Help()
            {
                return "Retrieve the help string for a named method";
            }
        };
    }
}
