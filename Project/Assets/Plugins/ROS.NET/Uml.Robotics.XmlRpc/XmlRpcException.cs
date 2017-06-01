using System;

namespace Uml.Robotics.XmlRpc
{
    public class XmlRpcException : Exception
    {
        public int ErrorCode { get; private set; }

        public XmlRpcException(string message, int errorCode = -1)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }
    }
}
