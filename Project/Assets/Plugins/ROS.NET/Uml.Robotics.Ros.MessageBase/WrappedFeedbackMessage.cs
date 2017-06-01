using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using std_msgs = Messages.std_msgs;
using actionlib_msgs = Messages.actionlib_msgs;

namespace Uml.Robotics.Ros
{
    [IgnoreRosMessage]
    public class WrappedFeedbackMessage<T> : RosMessage where T : InnerActionMessage, new()
    {
        public std_msgs.Header Header { get; set; } = new std_msgs.Header();
        public actionlib_msgs.GoalStatus GoalStatus { get; set; } = new actionlib_msgs.GoalStatus();
        protected T Content { get; set; } = new T();

        public WrappedFeedbackMessage()
            : base()
        {
        }

        public WrappedFeedbackMessage(byte[] serializedMessage)
        {
            Deserialize(serializedMessage);
        }

        public WrappedFeedbackMessage(byte[] serializedMessage, ref int currentIndex)
        {
            Deserialize(serializedMessage, ref currentIndex);
        }

        public override void Deserialize(byte[] serializedMessage, ref int currentIndex)
        {
            Header = new std_msgs.Header(serializedMessage, ref currentIndex);
            GoalStatus = new actionlib_msgs.GoalStatus(serializedMessage, ref currentIndex);
            Content = (T)Activator.CreateInstance(typeof(T), serializedMessage, currentIndex);
        }

        public override byte[] Serialize(bool partofsomethingelse)
        {
            List<byte[]> pieces = new List<byte[]>();

            if (Header == null)
                Header = new std_msgs.Header();
            pieces.Add(Header.Serialize(true));

            if (GoalStatus == null)
                GoalStatus = new actionlib_msgs.GoalStatus();
            pieces.Add(GoalStatus.Serialize(true));

            if (Content == null)
                Content = new T();
            pieces.Add(Content.Serialize(true));

            // combine every array in pieces into one array and return it
            int __a_b__f = pieces.Sum((__a_b__c) => __a_b__c.Length);
            int __a_b__e = 0;
            byte[] __a_b__d = new byte[__a_b__f];
            foreach (var __p__ in pieces)
            {
                Array.Copy(__p__, 0, __a_b__d, __a_b__e, __p__.Length);
                __a_b__e += __p__.Length;
            }
            return __a_b__d;
        }

        public bool Equals(WrappedFeedbackMessage<T> message)
        {
            if (message == null)
            {
                return false;
            }

            bool result = true;
            result &= Header.Equals(message.Header);
            result &= GoalStatus.Equals(message.GoalStatus);
            result &= Content.Equals(message.Content);
            return result;
        }

        public override bool Equals(RosMessage msg)
        {
            return Equals(msg as WrappedFeedbackMessage<T>);
        }

        public override void Randomize()
        {
            Header.Randomize();
            GoalStatus.Randomize();
            Content.Randomize();
        }

        protected string CalcMd5(string hashText)
        {
            string md5sum;
            using (var md5 = MD5.Create())
            {
                var md5Hash = md5.ComputeHash(Encoding.ASCII.GetBytes(hashText));
                var sb = new StringBuilder();
                for (int i = 0; i < md5Hash.Length; i++)
                {
                    sb.Append(md5Hash[i].ToString("x2"));
                }
                md5sum = sb.ToString();
            }

            return md5sum;
        }
    }
}
