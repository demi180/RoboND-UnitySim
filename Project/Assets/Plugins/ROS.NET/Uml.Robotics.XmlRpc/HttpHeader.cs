//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Uml.Robotics.XmlRpc
{
    internal enum HttpHeaderField
    {
        Accept = 0,
        Accept_Charset = 1,
        Accept_Encoding = 2,
        Accept_Language = 3,
        Accept_Ranges = 4,
        Authorization = 5,
        Cache_Control = 6,
        Connection = 7,
        Cookie = 8,
        Content_Length = 9,
        Content_Type = 10,
        Date = 11,
        Expect = 12,
        From = 13,
        Host = 14,
        If_Match = 15,
        If_Modified_Since = 16,
        If_None_Match = 17,
        If_Range = 18,
        If_Unmodified_Since = 19,
        Max_Forwards = 20,
        Pragma = 21,
        Proxy_Authorization = 22,
        Range = 23,
        Referer = 24,
        TE = 25,
        Upgrade = 26,
        User_Agent = 27,
        Via = 28,
        Warn = 29,
        Age = 30,
        Allow = 31,
        Content_Encoding = 32,
        Content_Language = 33,
        Content_Location = 34,
        Content_Disposition = 35,
        Content_MD5 = 36,
        Content_Range = 37,
        ETag = 38,
        Expires = 39,
        Last_Modified = 40,
        Location = 41,
        Proxy_Authenticate = 42,
        Refresh = 43,
        Retry_After = 44,
        Server = 45,
        Set_Cookie = 46,
        Trailer = 47,
        Transfer_Encoding = 48,
        Vary = 49,
        Warning = 50,
        WWW_Authenticate = 51,
        HEADER_VALUE_MAX_PLUS_ONE = 52
    };

    internal class HttpHeader
    {
        internal enum ParseStatus
        {
            UNINITIALIZED,
            PARTIAL_HEADER,
            COMPLETE_HEADER
        }

//        private ILogger Logger { get; } = XmlRpcLogging.CreateLogger<HttpHeader>();
        Dictionary<HttpHeaderField, string> m_StrHTTPField = new Dictionary<HttpHeaderField, string>();
        Dictionary<HttpHeaderField, string> HeaderFieldToStrings = new Dictionary<HttpHeaderField, string>();
        byte[] data = new byte[4096];
        string headerSoFar = "";
        ParseStatus headerStatus = ParseStatus.UNINITIALIZED;

        private HttpHeader()
        {
            this.DataString = "";
        }

        public HttpHeader(string request)
            : this()
        {
            Append(request);
        }

        public HttpHeader(byte[] binaryRequest)
            : this(Encoding.ASCII.GetString(binaryRequest))
        {
        }

        internal ParseStatus HeaderStatus
        {
            get { return headerStatus; }
        }

        public string Header
        {
            get { return headerSoFar; }
        }

        public Dictionary<HttpHeaderField, string> HTTPField
        {
            get { return m_StrHTTPField; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public string DataString { get; private set; }

        public int ContentLength
        {
            get
            {
                int ret;
                string value;
                if (m_StrHTTPField.TryGetValue(HttpHeaderField.Content_Length, out value) && int.TryParse(value, out ret))
                    return ret;
                return -1;
            }
        }

        public bool ContentComplete
        {
            get
            {
                int contentlength = ContentLength;
                if (contentlength <= 0)
                    return false;

                return this.DataString != null && this.DataString.Length >= contentlength;
            }
        }

        /// <summary>
        ///     Either HTTPRequest contains the header AND some data, or it contains part or all of the header. Accumulate pieces
        ///     of the header in case it spans multiple reads.
        /// </summary>
        /// <param name="HTTPRequest"></param>
        /// <returns></returns>
        public ParseStatus Append(string HTTPRequest)
        {
            if (headerStatus != ParseStatus.COMPLETE_HEADER)
            {
                int betweenHeaderAndData = HTTPRequest.IndexOf("\r\n\r\n");
                if (betweenHeaderAndData > 0)
                {
                    headerStatus = ParseStatus.COMPLETE_HEADER;
                    //found the boundary between header and data
                    headerSoFar += HTTPRequest.Substring(0, betweenHeaderAndData);
                    parseHeader(headerSoFar);

                    //shorten the request so we can fall through
                    HTTPRequest = HTTPRequest.Substring(betweenHeaderAndData + 4);
                    //
                    // FALL THROUGH to header complete case
                    //
                }
                else
                {
                    headerSoFar += HTTPRequest;
                    headerStatus = ParseStatus.PARTIAL_HEADER;
                    parseHeader(headerSoFar);
                    return headerStatus;
                }
            }

            if (headerStatus == ParseStatus.COMPLETE_HEADER)
            {
                if (ContentComplete)
                {
                    //this isn't right... restart with empty header and see if it works
                    headerStatus = ParseStatus.UNINITIALIZED;
                    Data = new byte[0];
                    DataString = "";
                    headerSoFar = "";
                    m_StrHTTPField.Clear();
                    return Append(HTTPRequest);
                }

                DataString += HTTPRequest;
                if (ContentComplete)
                {
                    Data = Encoding.ASCII.GetBytes(DataString);
//                    Logger.LogDebug("DONE READING CONTENT");
                }
            }

            return headerStatus;
        }

        // Fix for .net Core 1.0 IndexOf with StringComparison.OrdinalIgnoreCase bug.
        // see https://github.com/dotnet/corefx/issues/4587
        private static int IndexOfNoCase(string str, string part)
        {
            for (int i = 0; i <= str.Length - part.Length; ++i)
            {
                int j;
                for (j = 0; j < part.Length; ++j)
                {
                    if (char.ToLower(str[i + j]) != char.ToLower(part[j]))
                        break;
                }
                if (j == part.Length)
                    return i;
            }
            return -1;
        }

        private void parseHeader(string header)
        {
            HttpHeaderField HHField;
            string HTTPfield = null;
            int index;
            string buffer;
            for (int f = (int)HttpHeaderField.Accept; f < (int)HttpHeaderField.HEADER_VALUE_MAX_PLUS_ONE; f++)
            {
                HHField = (HttpHeaderField)f;
                HTTPfield = null;
                if (!HeaderFieldToStrings.TryGetValue(HHField, out HTTPfield) || HTTPField == null)
                {
                    HTTPfield = "\n" + HHField.ToString().Replace('_', '-') + ": ";
                    HeaderFieldToStrings.Add(HHField, HTTPfield);
                }

                index = IndexOfNoCase(header, HTTPfield);
                if (index == -1)
                    continue;

                buffer = header.Substring(index + HTTPfield.Length);
                index = buffer.IndexOf("\r\n");
                if (index == -1)
                    m_StrHTTPField[HHField] = buffer.Trim();
                else
                    m_StrHTTPField[HHField] = buffer.Substring(0, index).Trim();

                if (m_StrHTTPField[HHField].Length == 0)
                {
//                    Logger.LogWarning("HTTP HEADER: field \"{0}\" has a length of 0", HHField.ToString());
                }
//                Logger.LogDebug("HTTP HEADER: Index={0} | champ={1} = {2}", f, HTTPfield.Substring(1), m_StrHTTPField[HHField]);
            }
        }
    }
}
