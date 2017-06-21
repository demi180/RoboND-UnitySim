using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Uml.Robotics.Ros
{
    /// <summary>
    /// A callback queue which runs a background thread for calling callbacks. It does not need a spinner. The queue is disabled
    /// by default. The background thread is created when the queue gets enabled.
    /// </summary>
    public class CallbackQueueThreaded : ICallbackQueue
    {
        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<CallbackQueueThreaded>();
        private int count;
        private int calling;
        private Thread callbackThread;
        private bool enabled;
        private Dictionary<long, IDInfo> idInfo = new Dictionary<long, IDInfo>();
        private object idInfoMutex = new object();
        private AutoResetEvent sem = new AutoResetEvent(false);
        private object mutex = new object();
        private List<CallbackInfo> callbacks = new List<CallbackInfo>();
        private TLS tls;


        public bool IsEmpty
        {
            get { return count == 0; }
        }


        public bool IsEnabled
        {
            get { return enabled; }
        }


        private void SetupTls()
        {
            if (tls == null)
            {
                tls = new TLS
                {
                    calling_in_this_thread = Thread.CurrentThread.ManagedThreadId
                };
            }
        }


        internal void NotifyAll()
        {
            sem.Set();
        }


        internal void NotifyOne()
        {
            sem.Set();
        }


        public IDInfo GetIdInfo(long id)
        {
            lock (idInfoMutex)
            {
				IDInfo value;
				if ( idInfo.TryGetValue ( id, out value ) )
					return value;
            }
            return null;
        }


		public void AddCallback(CallbackInterface callback)
		{
			AddCallback ( callback, callback.Uid );
		}


        public void AddCallback(CallbackInterface cb, long owner_id)
        {
            CallbackInfo info = new CallbackInfo {Callback = cb, RemovalId = owner_id};
            //Logger.LogDebug($"CallbackQueue@{cbthread.ManagedThreadId}: Add callback owner: {owner_id} {cb.ToString()}");

            lock (mutex)
            {
                if (!enabled)
                    return;
                callbacks.Add(info);
                //Logger.LogDebug($"CallbackQueue@{cbthread.ManagedThreadId}: Added");
                count++;
            }
            lock (idInfoMutex)
            {
                if (!idInfo.ContainsKey(owner_id))
                {
                    idInfo.Add(owner_id, new IDInfo {calling_rw_mutex = new object(), id = owner_id});
                }
            }
            NotifyOne();
        }


        public void RemoveById(long ownerId)
        {
            SetupTls();
            IDInfo idinfo;
            lock (idInfoMutex)
            {
                if (!idInfo.ContainsKey(ownerId))
                    return;
                idinfo = idInfo[ownerId];
            }
            if (idinfo.id == tls.calling_in_this_thread)
                RemoveAll(ownerId);
            else
            {
                Logger.LogDebug("removeByID w/ WRONG THREAD ID");
                RemoveAll(ownerId);
            }
        }


        private void RemoveAll(long ownerId)
        {
            lock (mutex)
            {
                callbacks.RemoveAll(ici => ici.RemovalId == ownerId);
                count = callbacks.Count;
            }
        }


        private void ThreadFunc()
        {
            TimeSpan wallDuration = new TimeSpan(0, 0, 0, 0, ROS.WallDuration);
            while (ROS.ok)
            {
                DateTime begin = DateTime.UtcNow;
                CallAvailable(ROS.WallDuration);
                DateTime end = DateTime.UtcNow;

                var remainingTime = wallDuration - (end - begin);
                if (remainingTime > TimeSpan.Zero)
                    Thread.Sleep(remainingTime);
            }
            Logger.LogDebug("CallbackQueue thread broke out!");
        }


        public void Enable()
        {
            lock (mutex)
            {
                enabled = true;
            }
            NotifyAll();
            if (callbackThread == null)
            {
                callbackThread = new Thread(ThreadFunc);
                callbackThread.Start();
            }
        }


        public void Disable()
        {
            lock (mutex)
            {
                enabled = false;
            }
            NotifyAll();
            if (callbackThread != null)
            {
                callbackThread.Join();
                callbackThread = null;
            }
        }


        public void Clear()
        {
            lock (mutex)
            {
                callbacks.Clear();
                count = 0;
            }
        }


        public CallOneResult CallOne(TLS tls)
        {
            CallbackInfo info = tls.Head;
            if (info == null)
                return CallOneResult.Empty;
            IDInfo idinfo = null;
            idinfo = GetIdInfo(info.RemovalId);
            if (idinfo != null)
            {
                CallbackInterface cb = info.Callback;
                lock (idinfo.calling_rw_mutex)
                {
                    CallbackInterface.CallResult result = CallbackInterface.CallResult.Invalid;
                    tls.SpliceOut(info);
                    if (!info.MarkedForRemoval)
                    {
                        result = cb.Call();
                    }
                    if (result == CallbackInterface.CallResult.TryAgain && !info.MarkedForRemoval)
                    {
                        lock (mutex)
                        {
                            callbacks.Add(info);
                            count++;
                        }
                        return CallOneResult.TryAgain;
                    }
                }
                return CallOneResult.Called;
            }
            CallbackInfo cbi = tls.SpliceOut(info);
            if (cbi != null)
                cbi.Callback.Call();
            return CallOneResult.Called;
        }

        public void CallAvailable(int timeout)
        {
            SetupTls();
            int called = 0;
            lock (mutex)
            {
                if (!enabled)
                    return;
            }
            if (count == 0 && timeout != 0)
            {
                if (!sem.WaitOne(timeout))
                    return;
            }
            //Logger.LogDebug($"CallbackQueue@{cbthread.ManagedThreadId}: Enqueue TLS");
            lock (mutex)
            {
                if (count == 0)
                    return;
                if (!enabled)
                    return;
                callbacks.ForEach(cbi => tls.Enqueue(cbi));
                callbacks.Clear();
                count = 0;
                calling += tls.Count;
            }
            //Logger.LogDebug($"CallbackQueue@{cbthread.ManagedThreadId}: TLS count {tls.Count}");
            while (tls.Count > 0 && ROS.ok)
            {
                //Logger.LogDebug($"CallbackQueue@{cbthread.ManagedThreadId}: call {tls.head.Callback.ToString()}");
                if (CallOne(tls) != CallOneResult.Empty)
                    ++called;
            }
            lock (mutex)
            {
                calling -= called;
            }
            sem.Set();
        }
    }
}
