using System;
using System.Collections.Generic;
using System.Threading;
using Messages.rosgraph_msgs;

namespace Uml.Robotics.Ros
{
    internal class CallerInfo
    {
        public string MemberName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
    }

    public class RosOutAppender
    {
        public static RosOutAppender Instance
        {
            get { return instance.Value; }
        }

        internal enum ROSOUT_LEVEL
        {
            DEBUG = 1,
            INFO = 2,
            WARN = 4,
            ERROR = 8,
            FATAL = 16
        }

        private static Lazy<RosOutAppender> instance = new Lazy<RosOutAppender>(LazyThreadSafetyMode.ExecutionAndPublication);
        private Queue<Log> log_queue = new Queue<Log>();
        private Thread publish_thread;
        private bool shutting_down;
        private Publisher<Log> publisher;


        internal static void Terminate()
        {
            Instance.Shutdown();
        }


        internal static void Reset()
        {
            instance = new Lazy<RosOutAppender>(LazyThreadSafetyMode.ExecutionAndPublication);
        }


        public RosOutAppender()
        {
            publish_thread = new Thread(LogThread) { IsBackground = true };
        }


        public bool Started
        {
            get { return publish_thread != null && (publish_thread.ThreadState == System.Threading.ThreadState.Running || publish_thread.ThreadState == System.Threading.ThreadState.Background); }
        }


        public void Start()
        {
            if (!shutting_down && !Started)
            {
                if (publisher == null)
                    publisher = ROS.GlobalNodeHandle.advertise<Log>("/rosout", 0);
                publish_thread.Start();
            }
        }


        public void Shutdown()
        {
            shutting_down = true;
            if(Started)
            {
                publish_thread.Join();
            }
            if (publisher != null)
            {
                publisher.shutdown();
                publisher = null;
            }
        }


        internal void Append(string message, ROSOUT_LEVEL level, CallerInfo callerInfo)
        {
            var logMessage = new Log
            {
                msg = message,
                name = ThisNode.Name,
                file = callerInfo.FilePath,
                function = callerInfo.MemberName,
                line = (uint)callerInfo.LineNumber,
                level = ((byte)((int)level)),
                header = new Messages.std_msgs.Header() { stamp = ROS.GetTime() }
            };
            TopicManager.Instance.getAdvertisedTopics(out logMessage.topics);
            lock (log_queue)
                log_queue.Enqueue(logMessage);
        }


        private void LogThread()
        {
            Queue<Log> localqueue;
            while (!shutting_down)
            {
                lock (log_queue)
                {
                    localqueue = new Queue<Log>(log_queue);
                    log_queue.Clear();
                }
                while (!shutting_down && localqueue.Count > 0)
                {
                    publisher.publish(localqueue.Dequeue());
                }
                if (shutting_down)
                    return;
                Thread.Sleep(100);
            }
        }
    }
}
