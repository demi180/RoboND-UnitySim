using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace Uml.Robotics.XmlRpc
{
    public class XmlRpcCallResult
    {
        public bool Success { get; set; }
        public XmlRpcValue Value { get; set; }
    }

    public class XmlRpcClient
    {
        const string REQUEST_BEGIN = "<?xml version=\"1.0\"?>\r\n<methodCall><methodName>";
        const string REQUEST_END_METHODNAME = "</methodName>\r\n";
        const string PARAMS_TAG = "<params>";
        const string PARAMS_ETAG = "</params>";
        const string PARAM_TAG = "<param>";
        const string PARAM_ETAG = "</param>";
        const string REQUEST_END = "</methodCall>\r\n";

        Uri serverUri;

        public XmlRpcClient(Uri serverUri)
        {
            this.serverUri = serverUri;
        }

        public XmlRpcClient(string hostName, int port)
            : this(new UriBuilder("http", hostName, port, "/").Uri)
        {
        }

        public string Host
        {
            get { return serverUri.Host; }
        }

        public int Port
        {
            get { return serverUri.Port; }
        }

        public async Task<XmlRpcCallResult> ExecuteAsync(string methodName, XmlRpcValue parameters)
        {
            var req = HttpWebRequest.CreateHttp(serverUri);
            req.Method = "POST";
            req.ContentType = "text/xml; charset=utf-8";

            using (var stream = await req.GetRequestStreamAsync())
            {
                // serialize request into memory stream
                var buffer = new MemoryStream();
                var sw = new StreamWriter(buffer);       // by default uses UTF-8 encoding without BOM
                WriteRequest(sw, methodName, parameters);
                sw.Flush();
                buffer.Position = 0;

                await buffer.CopyToAsync(stream);
            }

            using (var response = await req.GetResponseAsync())
            {
                var encoding = Encoding.UTF8;
                using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    var responseText = await reader.ReadToEndAsync();
                    return ParseResponse(responseText);
                }
            }
        }

        public XmlRpcCallResult Execute(string methodName, XmlRpcValue parameters)
        {
            return ExecuteAsync(methodName, parameters).Result;
        }

        private void WriteRequest(StreamWriter writer, string methodName, XmlRpcValue parameters)
        {
            writer.Write(REQUEST_BEGIN + methodName + REQUEST_END_METHODNAME);

            if (parameters.IsEmpty)
            {
                writer.Write(PARAMS_TAG);

                if (parameters.Type == XmlRpcType.Array)
                {
                    // If params is an array, each element is a separate parameter
                    for (int i = 0; i < parameters.Count; ++i)
                    {
                        writer.Write(PARAM_TAG);
                        writer.Write(parameters[i].ToXml());
                        writer.Write(PARAM_ETAG);
                    }
                }
                else
                {
                    writer.Write(PARAM_TAG);
                    writer.Write(parameters.ToXml());
                    writer.Write(PARAM_ETAG);
                }

                writer.Write(PARAMS_ETAG);
            }

            writer.Write(REQUEST_END);
        }


        /// <summary>
        // Parse the server response XML into an XmlRpcCallResult.
        /// </summary>
        /// <param name="responseText">A string that contains the response receiveld from the server.</param>
        /// <returns>An XmlRpcCallResult holding the response status and the returned XmlRpcValue.</returns>
        private XmlRpcCallResult ParseResponse(string responseText)
        {
            var responseDocument = XDocument.Parse(responseText);
            var methodResponseElement = responseDocument.Element("methodResponse");
            if (methodResponseElement == null)
                throw new XmlRpcException("Expected <methodResponse> element is missing.");

            var paramsElement = methodResponseElement.Element("params");
            var faultElement = methodResponseElement.Element("fault");

            var result = new XmlRpcValue();
            if (paramsElement != null)
            {
                var selection = paramsElement.Elements("param").ToList();
                if (selection.Count > 1)
                {
                    result.SetArray(selection.Count);
                    for (int i = 0; i < selection.Count; i++)
                    {
                        var value = new XmlRpcValue();
                        value.FromXElement(selection[i].Element("value"));
                        result.Set(i, value);
                    }
                }
                else if (selection.Count == 1)
                {
                    result.FromXElement(selection[0].Element("value"));
                }
                else
                {
                    return new XmlRpcCallResult { Value = result, Success = false };
                }
            }
            else if (faultElement != null)
            {
                result.FromXElement(faultElement.Element("value"));
                return new XmlRpcCallResult { Value = result, Success = false };
            }
            else
            {
                throw new XmlRpcException("Invalid response - no param or fault tag found.");
            }

            return new XmlRpcCallResult { Value = result, Success = true };
        }
    }
}
