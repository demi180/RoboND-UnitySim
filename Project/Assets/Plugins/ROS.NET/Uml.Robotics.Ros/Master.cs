using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Uml.Robotics.XmlRpc;

namespace Uml.Robotics.Ros
{
    public static class Master
    {
        // cannot use the usubal CreateLogger<master>(); here because this calls is static
        private static ILogger Logger {get;} = ApplicationLogging.CreateLogger(nameof(Master));

        private static int port;
        private static string host;
        private static string uri;
        public static TimeSpan retryTimeout = TimeSpan.FromSeconds(5);

        public static void init(IDictionary<string, string> remapping_args)
        {
            uri = string.Empty;
            if (remapping_args.ContainsKey("__master"))
            {
                uri = (string) remapping_args["__master"];
                ROS.ROS_MASTER_URI = uri;
            }
            if (string.IsNullOrEmpty(uri))
                uri = ROS.ROS_MASTER_URI;
            if (!Network.SplitUri(uri, out host, out port))
            {
                port = 11311;
            }
        }

        /// <summary>
        ///     Check if ROS master is running by querying the PID of the master process.
        /// </summary>
        /// <returns></returns>
        public static bool check()
        {
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, ThisNode.Name);
            return execute("getPid", args, result, payload, false);
        }

        /// <summary>
        ///     Gets all currently published and subscribed topics and adds them to the topic list
        /// </summary>
        /// <param name="topics"> List to store topics</param>
        /// <returns></returns>
        public static bool getTopics(ref TopicInfo[] topics)
        {
            List<TopicInfo> topicss = new List<TopicInfo>();
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, ThisNode.Name);
            args.Set(1, "");
            if (!execute("getPublishedTopics", args, result, payload, true))
                return false;

            topicss.Clear();
            for (int i = 0; i < payload.Count; i++)
                topicss.Add(new TopicInfo(payload[i][0].GetString(), payload[i][1].GetString()));
            topics = topicss.ToArray();
            return true;
        }

        /// <summary>
        ///     Gets all currently existing nodes and adds them to the nodes list
        /// </summary>
        /// <param name="nodes">List to store nodes</param>
        /// <returns></returns>
        public static bool getNodes(ref string[] nodes)
        {
            List<string> names = new List<string>();
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, ThisNode.Name);

            if (!execute("getSystemState", args, result, payload, true))
            {
                return false;
            }
            for (int i = 0; i < payload.Count; i++)
            {
                for (int j = 0; j < payload[i].Count; j++)
                {
                    XmlRpcValue val = payload[i][j][1];
                    for (int k = 0; k < val.Count; k++)
                    {
                        string name = val[k].GetString();
                        names.Add(name);
                    }
                }
            }
            nodes = names.ToArray();
            return true;
        }

        internal static XmlRpcClient clientForNode(string nodename)
        {
            var args = new XmlRpcValue(ThisNode.Name, nodename);
            var resp = new XmlRpcValue();
            var payl = new XmlRpcValue();
            if (!execute("lookupNode", args, resp, payl, true))
                return null;

            if (!XmlRpcManager.Instance.ValidateXmlRpcResponse("lookupNode", resp, payl))
                return null;

            string nodeUri = payl.GetString();
            if (!Network.SplitUri(nodeUri, out string nodeHost, out int nodePort) || nodeHost == null || nodePort <= 0)
                return null;

            return new XmlRpcClient(nodeHost, nodePort);
        }

        public static bool kill(string node)
        {
            var cl = clientForNode(node);
            if (cl == null)
                return false;

            XmlRpcValue req = new XmlRpcValue(), resp = new XmlRpcValue(), payl = new XmlRpcValue();
            req.Set(0, ThisNode.Name);
            req.Set(1, $"Node '{ThisNode.Name}' requests shutdown.");
            var respose = cl.Execute("shutdown", req);
            if (!respose.Success || !XmlRpcManager.Instance.ValidateXmlRpcResponse("shutdown", respose.Value, payl))
                return false;

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="request">Full request to send to the master </param>
        /// <param name="waitForMaster">If you recieve an unseccessful status code, keep retrying.</param>
        /// <param name="response">Full response including status code and status message. Initially empty.</param>
        /// <param name="payload">Location to store the actual data requested, if any.</param>
        /// <returns></returns>
        public static bool execute(string method, XmlRpcValue request, XmlRpcValue response, XmlRpcValue payload, bool waitForMaster)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow;
                string master_host = host;
                int master_port = port;

                var client = new XmlRpcClient(master_host, master_port);
                bool printed = false;

                for (;;)
                {
                    // Check if we are shutting down
                    if (XmlRpcManager.Instance.IsShuttingDown)
                        return false;

                    try
                    {
                        var result = client.Execute(method, request);           // execute the RPC call
                        response.Set(result.Value);
                        if (result.Success)
                        {
                            // validateXmlrpcResponse logs error in case of validation error
                            // So we don't need any logging here.
                            if (XmlRpcManager.Instance.ValidateXmlRpcResponse(method, result.Value, payload))
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            if (response.IsArray && response.Count >= 2)
                                Logger.LogError("Execute failed: return={0}, desc={1}", response[0].GetInt(), response[1].GetString());
                            else
                                Logger.LogError("response type: " + response.Type.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        // no connection to ROS Master
                        if (waitForMaster)
                        {
                            if (!printed)
                            {
                                Logger.LogWarning(
                                    "[{0}] Could not connect to master at [{1}:{2}]. " +
                                    "Retrying for the next {3} seconds.",
                                    method, master_host, master_port,
                                    retryTimeout.TotalSeconds);
                                printed = true;
                            }

                            // timeout expired, throw exception
                            if (retryTimeout.TotalSeconds > 0 && DateTime.UtcNow.Subtract(startTime) > retryTimeout)
                            {
                                Logger.LogError("[{0}] Timed out trying to connect to the master [{1}:{2}] after [{1}] seconds",
                                                method, master_host, master_port, retryTimeout.TotalSeconds);

                                throw new RosException(String.Format("Cannot connect to ROS Master at {0}:{1}", master_host, master_port), ex);
                            }
                        }
                        else
                        {
                            throw new RosException(String.Format("Cannot connect to ROS Master at {0}:{1}", master_host, master_port), ex);
                        }

                    }

                    // recreate the client, thereby causing it to reinitiate its connection
                    Thread.Sleep(250);
                    client = new XmlRpcClient(master_host, master_port);
                }
            }
            catch (ArgumentNullException e)
            {
                Logger.LogError(e.ToString());
            }
            Logger.LogError("Master API call: {0} failed!\n\tRequest:\n{1}", method, request);
            return false;
        }
    }
}
