using Ros_CSharp;
//using ROS;
using Messages;
using Messages.std_msgs;
using System.Collections.Generic;

namespace Ros_CSharp
{
	public class MessageEvent<T> where T : IRosMessage
	{
		protected MessageEvent () {}
		public MessageEvent (T msg)
		{
			message = msg;
			connectionHeader = new Dictionary<string, string> ();
			receiptTime = ROS.GetTime ();
		}

//		public MessageEvent (T msg, string header)
//		{
//			message = msg;
//			connectionHeader = header;
//			receiptTime = ROS.GetTime ();
//		}

		IRosMessage message;
		Dictionary<string, string> connectionHeader;
		Time receiptTime;

		public IRosMessage getMessage () { return message; }
		public string getConnectionHeader ()
		{
			string s = "unknown_publisher";
			if ( connectionHeader != null )
				connectionHeader.TryGetValue ( "callerid", out s );
			return s;
		}
		public Time getReceiptTime () { return receiptTime; }
	}
}