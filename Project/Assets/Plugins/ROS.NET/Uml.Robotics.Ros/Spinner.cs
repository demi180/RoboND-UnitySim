//using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uml.Robotics.Ros
{
    public class SingleThreadSpinner
    {
        ICallbackQueue callbackQueue;
//        private ILogger Logger { get; } = ApplicationLogging.CreateLogger<SingleThreadSpinner>();


        /// <summary>
        /// Creats a spinner for the global ROS callback queue
        /// </summary>
        public SingleThreadSpinner()
        {
            this.callbackQueue = ROS.GlobalCallbackQueue;
        }


        /// <summary>
        /// Creates a spinner for the given callback queue
        /// </summary>
        /// <param name="callbackQueue"></param>
        public SingleThreadSpinner(ICallbackQueue callbackQueue)
        {
            this.callbackQueue = callbackQueue;
        }


        public void Spin()
        {
            Spin(CancellationToken.None);
//            Logger.LogCritical("CallbackQueue thread broke out! This only can happen if ROS.ok is false.");
        }


        public void Spin(CancellationToken token)
        {
            TimeSpan wallDuration = new TimeSpan(0, 0, 0, 0, ROS.WallDuration);
//            Logger.LogInformation("Start spinning");
            while (ROS.ok)
            {
                DateTime begin = DateTime.UtcNow;
                callbackQueue.CallAvailable(ROS.WallDuration);
    
                if (token.IsCancellationRequested)
                    break;

                DateTime end = DateTime.UtcNow;
                var remainingTime = wallDuration - (end - begin);
                if (remainingTime > TimeSpan.Zero)
                    Thread.Sleep(remainingTime);
            }
        }


        public void SpinOnce()
        {
            callbackQueue.CallAvailable(ROS.WallDuration);
        }
    }


    public class AsyncSpinner : IDisposable
    {
        private ICallbackQueue callbackQueue;
        private Task spinTask;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken token;


        /// <summary>
        /// Creates a spinner for the global ROS callback queue
        /// </summary>
        public AsyncSpinner()
        {
            this.callbackQueue = ROS.GlobalCallbackQueue;
        }


        /// <summary>
        /// Create a spinner for the given callback queue
        /// </summary>
        /// <param name="callbackQueue"></param>
        public AsyncSpinner(ICallbackQueue callbackQueue)
        {
            this.callbackQueue = callbackQueue;
        }

        public void Dispose()
        {
            tokenSource.Dispose();
        }

        public void Start()
        {
            spinTask = Task.Factory.StartNew(() =>
            {
                token = tokenSource.Token;
                var spinner = new SingleThreadSpinner(callbackQueue);
                spinner.Spin(token);
            });
        }

        public void Stop()
        {
            if (spinTask != null)
            {
                tokenSource.Cancel();
                spinTask = null;
            }
        }
    }
}