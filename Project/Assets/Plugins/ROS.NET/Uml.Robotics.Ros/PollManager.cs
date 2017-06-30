using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
//using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    public class PollSignal : IDisposable
    {
        public MethodInfo Method;
        public object Target;
        public delegate void PollSignalFunc();

        internal static event PollSignalFunc SignalEvent;

        private Thread thread;
        private Action _op;
        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        private bool disposed = false;

        /// <summary>
        /// Sets this Poll_Signal's periodic operation, AND makes it be auto-polled by PollManager.
        /// </summary>
        public Action Op
        {
            get
            {
                return _op;
            }

            set
            {
                ManualOp = value;
                if (value != null)
                {
                    SignalEvent += ContinueThreads;
                }
                else
                {
                    SignalEvent -= ContinueThreads;
                }
            }
        }

        /// <summary>
        /// Sets this Poll_Signal's operation, without making it be auto-polled by PollManager
        /// </summary>
        public Action ManualOp
        {
            get
            {
                return _op;
            }
            set
            {
                try
                {
                    SignalEvent -= ContinueThreads;
                }
                catch { }

               Method = value.GetMethodInfo();
               Target = value.Target;
                _op = value;
            }
        }


        public PollSignal(Action psf)
        {
            if (psf != null)
            {
                Op = psf;
            }
            thread = new Thread(ThreadFunc) { IsBackground = true };
            thread.Start();
        }


        internal void ContinueThreads()
        {
            resetEvent.Set();
        }


        private void ThreadFunc()
        {
            while (ROS.ok && !disposed)
            {
                resetEvent.WaitOne();
                if (ROS.ok && !disposed)
                    Op();
            }
            thread = null;
        }


        internal static void Signal()
        {
            if (SignalEvent != null)
                SignalEvent();
        }


        public void Dispose()
        {
            SignalEvent -= ContinueThreads;
            disposed = true;
            do
            {
                ContinueThreads();
            } while (thread != null && !thread.Join(1));
        }
    }


    public class PollManager
    {
        public PollSet poll_set;
        public bool shutting_down;
        public object signal_mutex = new object();
        public TcpTransport tcpserver_transport;

        public static PollManager Instance
        {
            get { return instance.Value; }
        }

//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<PollManager>();
        private static Lazy<PollManager> instance = new Lazy<PollManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        private List<PollSignal> signals = new List<PollSignal>();
        private Thread thread;


        internal static void Terminate()
        {
            Instance.Shutdown();
        }


        internal static void Reset()
        {
            instance = new Lazy<PollManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        }


        public PollManager()
        {
            poll_set = new PollSet();
        }


        public void AddPollThreadListener(Action poll)
        {
//            Logger.LogDebug("Adding pollthreadlistener " + poll.Target + ":" + poll.GetMethodInfo().Name);
            lock (signal_mutex)
            {
                signals.Add(new PollSignal(poll));
            }
            CallSignal();
        }


        private void CallSignal()
        {
            PollSignal.Signal();
        }


        public void RemovePollThreadListener(Action poll)
        {
            lock (signal_mutex)
            {
                signals.RemoveAll((s) => s.Op == poll);
            }
            CallSignal();
        }


        private void ThreadFunc()
        {
            while (!shutting_down)
            {
                CallSignal();
                Thread.Sleep(ROS.WallDuration);
                if (shutting_down)
                    return;
            }
//            Logger.LogDebug("PollManager thread finished");
        }


        public void Start()
        {
            if (thread == null)
            {
                shutting_down = false;
                thread = new Thread(ThreadFunc);
                thread.Start();
            }
        }


        public void Shutdown()
        {
            if (thread != null && !shutting_down)
            {
                shutting_down = true;
                poll_set.Dispose();
                poll_set = null;
                signals.Clear();
                if (!thread.Join(2000))
                {
//                    Logger.LogError("thread.Join() timed out.");
                }
                thread = null;
            }
        }
    }
}
