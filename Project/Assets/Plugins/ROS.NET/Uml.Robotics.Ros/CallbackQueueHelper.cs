using System;
using System.Collections.Generic;
using System.Text;

namespace Uml.Robotics.Ros
{
    public class CallbackInfo
    {
        public CallbackInterface Callback { get; set; }
        public bool MarkedForRemoval { get; set; }
        public long RemovalId { get; set; }
    }


    public class TLS
    {
        private readonly List<CallbackInfo> _queue = new List<CallbackInfo>();
        public int calling_in_this_thread = -1;

        public int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }

        public CallbackInfo Head
        {
            get
            {
                lock (_queue)
                {
                    if (_queue.Count == 0)
                        return null;

                    return _queue[0];
                }
            }
        }

        public CallbackInfo Tail
        {
            get
            {
                lock (_queue)
                {
                    if (_queue.Count == 0)
                        return null;

                    return _queue[_queue.Count - 1];
                }
            }
        }

        public CallbackInfo Dequeue()
        {
            lock (_queue)
            {
                if (_queue.Count == 0)
                    return null;

                CallbackInfo result = _queue[0];
                _queue.RemoveAt(0);
                return result;
            }
        }

        public void Enqueue(CallbackInfo info)
        {
            if (info.Callback == null)
                return;

            lock (_queue)
            {
                _queue.Add(info);
            }
        }

        public CallbackInfo SpliceOut(CallbackInfo info)
        {
            lock (_queue)
            {
                if (!_queue.Contains(info))
                    return null;
                _queue.RemoveAt(_queue.IndexOf(info));
                return info;
            }
        }
    }


    public class IDInfo
    {
        public object calling_rw_mutex;
        public long id;
    }


    public enum CallOneResult
    {
        Called,
        TryAgain,
        Disabled,
        Empty
    }
}
