using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uml.Robotics.XmlRpc;

namespace rosmaster
{
    public class ReturnStruct
    {

        public int statusCode;
        public String statusMessage;
        public XmlRpcValue value;

        public ReturnStruct(int _statusCode = 1, String _statusMessage = "", XmlRpcValue _value = null)
        {
            statusCode = _statusCode;
            statusMessage = _statusMessage;
            value = _value;
        }
    }
}
