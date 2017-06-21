using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.Loader;

using Microsoft.Extensions.Logging;
using Uml.Robotics.XmlRpc;
using std_msgs = Messages.std_msgs;
using System.IO;
using System.Threading.Tasks;

namespace Uml.Robotics.Ros
{
    /// <summary>
    /// A static class for global variables, initializations and shutdowns.
    /// </summary>
    public static class ROS
    {
        private static ILogger Logger { get; set; } = ApplicationLogging.CreateLogger(nameof(ROS));

        private static ICallbackQueue globalCallbackQueue;
        private static object startMutex = new object();

        public static TimerManager timerManager = new TimerManager();

        private static Task shutdownTask;
        internal static bool initialized;
        private static bool started;
        private static bool atExitRegistered;
        private static volatile bool _ok;
        internal static bool shuttingDown;
        private static volatile bool shutdownRequested;
        private static int initOptions;

        public static ICallbackQueue GlobalCallbackQueue
        {
			get { return globalCallbackQueue; }
        }

        /// <summary>
        ///     Means of setting ROS_MASTER_URI programatically before Init is called
        ///     Order of precedence: __master:=... > this variable > User Environment Variable > System Environment Variable
        /// </summary>
        public static string ROS_MASTER_URI { get; set; }

        /// <summary>
        ///     Means of setting ROS_HOSTNAME directly before Init is called
        ///     Order of precedence: __hostname:=... > this variable > User Environment Variable > System Environment Variable
        /// </summary>
        public static string ROS_HOSTNAME { get; set; }

        /// <summary>
        ///     Means of setting ROS_IP directly before Init is called
        ///     Order of precedence: __ip:=... > this variable > User Environment Variable > System Environment Variable
        /// </summary>
        public static string ROS_IP { get; set; }


        /// <summary>
        /// General global sleep time in miliseconds for spin operations.
        /// </summary>
        public const int WallDuration = 10;

        public static NodeHandle GlobalNodeHandle;
        private static object shuttingDownMutex = new object();

        private static TimeSpan lastSimTime;                // last sim time time
        private static TimeSpan lastSimTimeReceived;        // last sim time received time (wall)

        private const string ROSOUT_FMAT = "{0} {1}";
        private const string ROSOUT_DEBUG_PREFIX = "[Debug]";
        private const string ROSOUT_INFO_PREFIX  = "[Info ]";
        private const string ROSOUT_WARN_PREFIX  = "[Warn ]";
        private const string ROSOUT_ERROR_PREFIX = "[Error]";
        private const string ROSOUT_FATAL_PREFIX = "[FATAL]";

        private static Dictionary<RosOutAppender.ROSOUT_LEVEL, string> ROSOUT_PREFIX =
            new Dictionary<RosOutAppender.ROSOUT_LEVEL, string> {
                { RosOutAppender.ROSOUT_LEVEL.DEBUG, ROSOUT_DEBUG_PREFIX },
                { RosOutAppender.ROSOUT_LEVEL.INFO, ROSOUT_INFO_PREFIX },
                { RosOutAppender.ROSOUT_LEVEL.WARN, ROSOUT_WARN_PREFIX },
                { RosOutAppender.ROSOUT_LEVEL.ERROR, ROSOUT_ERROR_PREFIX },
                { RosOutAppender.ROSOUT_LEVEL.FATAL, ROSOUT_FATAL_PREFIX }
            };

        public static bool shutting_down
        {
            get { return shuttingDown; }
        }

        /// <summary>
        ///     True if ROS is ok, false if not
        /// </summary>
        public static bool ok
        {
            get { return _ok; }
        }

        /// <summary>
        ///     Turns a DateTime into a Time struct
        /// </summary>
        /// <param name="time"> DateTime to convert </param>
        /// <returns> containing secs, nanosecs since 1/1/1970 </returns>
        public static std_msgs.Time GetTime(DateTime time)
        {
            return GetTime<std_msgs.Time>(time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)));
        }

        /// <summary>
        ///     Turns a std_msgs.Time into a DateTime
        /// </summary>
        /// <param name="time"> std_msgs.Time to convert </param>
        /// <returns> a DateTime </returns>
        public static DateTime GetTime(std_msgs.Time time)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).Add(new TimeSpan(time.data.Ticks));
        }

        /// <summary>
        ///     Turns a std_msgs.Duration into a TimeSpan
        /// </summary>
        /// <param name="time"> std_msgs.Duration to convert </param>
        /// <returns> a TimeSpan </returns>
        public static TimeSpan GetTime(std_msgs.Duration duration)
        {
            return new TimeSpan(duration.data.Ticks);
        }

        public static T GetTime<T>(TimeSpan ts) where T : RosMessage, new()
        {
            T test = Activator.CreateInstance(typeof(T), GetTime(ts)) as T;
            return test;
        }

        /// <summary>
        ///     Turns a TimeSpan into a Time (not a Duration, although it sorta is)
        /// </summary>
        /// <param name="timestamp"> The timespan to convert to seconds/nanoseconds </param>
        /// <returns> a time struct </returns>
        public static TimeData GetTime(TimeSpan timestamp)
        {
            if (lastSimTimeReceived != default(TimeSpan))
            {
                timestamp = timestamp.Subtract(lastSimTimeReceived).Add(lastSimTime);
            }
            return TimeData.FromTicks(timestamp.Ticks);
        }

        /// <summary>
        ///     Gets the current time as secs/nsecs
        /// </summary>
        /// <returns> </returns>
        public static std_msgs.Time GetTime()
        {
            return GetTime(DateTime.UtcNow);
        }

        private static void SimTimeCallback(TimeSpan ts)
        {
            lastSimTime = ts;
            lastSimTimeReceived = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        }

        /// <summary>
        /// Non-creatable marker class
        /// </summary>
        public class ONLY_AUTO_PARAMS
        {
            private ONLY_AUTO_PARAMS() {}
        }

        public delegate void WriteDelegate(object format, params object[] args);

        /// <summary>
        ///     ROS_INFO(...)
        /// </summary>
        public static WriteDelegate Info(ONLY_AUTO_PARAMS CAPTURE_CALL_SITE = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return (format, args) => _rosout(format, args, RosOutAppender.ROSOUT_LEVEL.INFO, new CallerInfo { MemberName = memberName, FilePath = filePath, LineNumber = lineNumber });
        }

        /// <summary>
        ///     ROS_DEBUG(...) (formatted)
        /// </summary>
        public static WriteDelegate Debug(ONLY_AUTO_PARAMS CAPTURE_CALL_SITE = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return (format, args) => _rosout(format, args, RosOutAppender.ROSOUT_LEVEL.DEBUG, new CallerInfo { MemberName = memberName, FilePath = filePath, LineNumber = lineNumber });
        }

        /// <summary>
        ///     ROS_INFO(...) (formatted)
        /// </summary>
        public static WriteDelegate Error(ONLY_AUTO_PARAMS CAPTURE_CALL_SITE = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return (format, args) => _rosout(format, args, RosOutAppender.ROSOUT_LEVEL.ERROR, new CallerInfo { MemberName = memberName, FilePath = filePath, LineNumber = lineNumber });
        }

        /// <summary>
        ///     ROS_WARN(...) (formatted)
        /// </summary>
        public static WriteDelegate Warn(ONLY_AUTO_PARAMS CAPTURE_CALL_SITE = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return (format, args) => _rosout(format, args, RosOutAppender.ROSOUT_LEVEL.WARN, new CallerInfo { MemberName = memberName, FilePath = filePath, LineNumber = lineNumber });
        }

        private static void _rosout(object format, object[] args, RosOutAppender.ROSOUT_LEVEL level, CallerInfo callerInfo)
        {
            using (Logger.BeginScope(nameof(_rosout)))
            {
                if (format == null)
                    throw new ArgumentNullException(nameof(format));

                string text = (args == null || args.Length == 0) ? format.ToString() : string.Format((string)format, args);

                bool printit = true;
                if (level == RosOutAppender.ROSOUT_LEVEL.DEBUG)
                {
    #if !DEBUG
                    printit = false;
    #endif
                }
                if (printit)
                    Logger.LogDebug(ROSOUT_FMAT, ROSOUT_PREFIX[level], text);
                RosOutAppender.Instance.Append(text, level, callerInfo);
            }
        }

        /// <summary>
        ///     Set the logging factory for ROS.NET
        /// </summary>
        /// <param name="factory"> The logging factory to use for logging </param>
        public static void SetLoggerFactory(ILoggerFactory factory)
        {
            ApplicationLogging.LoggerFactory = factory;

            // recreate logger to make sure the new log settings form the factory are used
            Logger = ApplicationLogging.CreateLogger(nameof(ROS));
            if (initialized)
            {
                Logger.LogWarning("Logging should be configured before initializing the ROS system.");
            }
        }

        /// <summary>
        ///     Initializes ROS so nodehandles and nodes can exist
        /// </summary>
        /// <param name="args"> argv - parsed for remapping args (AND PARAMS??) </param>
        /// <param name="name"> the node's name </param>
        public static void Init(string[] args, string name)
        {
            Init(args, name, 0);
        }

        /// <summary>
        ///     Initializes ROS so nodehandles and nodes can exist
        /// </summary>
        /// <param name="args"> argv - parsed for remapping args (AND PARAMS??) </param>
        /// <param name="name"> the node's name </param>
        /// <param name="options"> options? </param>
        public static void Init(string[] args, string name, int options)
        {
            // ROS_MASTER_URI/ROS_HOSTNAME definition precedence:
            // 1. explicitely set by program
            // 2. passed in as remap argument
            // 3. environment variable

			IDictionary<string, string> remapping;
            if (RemappingHelper.GetRemappings(ref args, out remapping))
                Init(remapping, name, options);
            else
                throw new InvalidOperationException("Could not initialize ROS");
        }

        /// <summary>
        ///     Initializes ROS so nodehandles and nodes can exist
        /// </summary>
        /// <param name="remappingArgs"> dictionary of remapping args </param>
        /// <param name="name"> node name </param>
        /// <param name="options"> options </param>
        public static void Init(IDictionary<string, string> remappingArgs, string name, int options = 0)
        {
            lock (typeof(ROS))
            {
                // register process unload and cancel (CTRL+C) event handlers
                if (!atExitRegistered)
                {
                    atExitRegistered = true;
                    AssemblyLoadContext.Default.Unloading += (AssemblyLoadContext obj) =>
                    {
                        shutdown();
                        waitForShutdown();
                    };

                    Console.CancelKeyPress += (o, args) =>
                    {
                        shutdown();
                        waitForShutdown();
                        args.Cancel = true;
                    };
                }

                // crate global callback queue
                if (globalCallbackQueue == null)
                {
                    globalCallbackQueue = new CallbackQueue();
                }

                // run the actual ROS initialization
                if (!initialized)
                {
                    MessageTypeRegistry.Default.Reset();
                    ServiceTypeRegistry.Default.Reset();
                    var msgRegistry = MessageTypeRegistry.Default;
                    var srvRegistry = ServiceTypeRegistry.Default;

                    // Load RosMessages from MessageBase assembly
                    msgRegistry.ParseAssemblyAndRegisterRosMessages(typeof(RosMessage).GetTypeInfo().Assembly);

                    // Load RosMessages from all assemblies that depend on MessageBase
                    var candidates = MessageTypeRegistry.GetCandidateAssemblies("Uml.Robotics.Ros.MessageBase");
                    foreach (var assembly in candidates)
                    {
                        Logger.LogDebug($"Parse assembly: {assembly.Location}");
                        msgRegistry.ParseAssemblyAndRegisterRosMessages(assembly);
                        srvRegistry.ParseAssemblyAndRegisterRosServices(assembly);
                    }

                    initOptions = options;
                    _ok = true;

                    Param.Reset();
                    SimTime.Reset();
                    RosOutAppender.Reset();

                    Network.Init(remappingArgs);
                    Master.init(remappingArgs);
                    ThisNode.Init(name, remappingArgs, options);
                    Param.Init(remappingArgs);
                    SimTime.Instance.SimTimeEvent += SimTimeCallback;

                    lock (shuttingDownMutex)
                    {
						if ( shutdownTask != null )
						{
							switch ( shutdownTask.Status )
							{
							case null:
							case TaskStatus.RanToCompletion:
								break;
							default:
								throw new InvalidOperationException ( "ROS was not shut down correctly" );
							}
						}
                        shutdownTask = new Task(_shutdown);
                    }
                    initialized = true;

					GlobalNodeHandle = new NodeHandle ( ThisNode.Namespace, remappingArgs );
					RosOutAppender.Instance.Start ();
                }
            }
        }

        /// <summary>
        ///     shutdowns are async with the call to shutdown. This delays shutting down ROS feels like it.
        /// </summary>
        internal static void CheckForShutdown()
        {
            lock (shuttingDownMutex)
            {
                if (!shutdownRequested || shutdownTask == null || shutdownTask.Status != TaskStatus.Created)
                    return;
                shutdownTask.Start();
            }
        }

        /// <summary>
        ///     This is called when rosnode kill is invoked
        /// </summary>
        /// <param name="p"> pointer to unmanaged XmlRpcValue containing params </param>
        /// <param name="r"> pointer to unmanaged XmlRpcValue that will contain return value </param>
        private static void ShutdownCallback(XmlRpcValue parms, XmlRpcValue r)
        {
            int num_params = 0;
            if (parms.Type == XmlRpcType.Array)
                num_params = parms.Count;
            if (num_params > 1)
            {
                string reason = parms[1].GetString();
                Logger.LogInformation("Shutdown request received.");
                Logger.LogInformation("Reason given for shutdown: [" + reason + "]");
                shutdown();
            }
            XmlRpcManager.ResponseInt(1, "", 0)(r);
        }

        /// <summary>
        ///     Hang the current thread until ROS shuts down
        /// </summary>
        public static void waitForShutdown()
        {
            if (shutdownTask == null)
            {
                throw new NullReferenceException($"{nameof(shutdownTask)} was not initialized. You need to call ROS.init first.");
            }
            shutdownTask.Wait();
        }


        /// <summary>
        ///     Finishes intialization This is called by the first NodeHandle when it initializes
        /// </summary>
        internal static void Start()
        {
            lock (startMutex)
            {
                if (started)
                    return;

                PollManager.Reset();
                ServiceManager.Reset();
                XmlRpcManager.Reset();
                TopicManager.Reset();
                ConnectionManager.Reset();

                PollManager.Instance.AddPollThreadListener(CheckForShutdown);
                XmlRpcManager.Instance.Bind("shutdown", ShutdownCallback);
                TopicManager.Instance.Start();
                ServiceManager.Instance.Start();
                ConnectionManager.Instance.Start();
                PollManager.Instance.Start();
                XmlRpcManager.Instance.Start();

                shutdownRequested = false;
                shuttingDown = false;
                started = true;
                _ok = true;
            }
        }

        /// <summary>
        /// Check whether ROS.init() has been called.
        /// </summary>
        /// <returns>System.Boolean indicating whether ROS has been started.</returns>
        public static bool isStarted()
        {
            return started;
        }

        /// <summary>
        ///     Tells ROS that it should shutdown the next time it feels like doing so.
        /// </summary>
        public static Task shutdown()
        {
            lock (shuttingDownMutex)
            {
                if ((shutdownTask == null) || shutdownTask.Status == TaskStatus.Created)
                {
                    shutdownRequested = true;
                }
            }

            return shutdownTask;
        }


        /// <summary>
        /// Internal ROS deinitialization method. Called by checkForShutdown.
        /// </summary>
        private static void _shutdown()
        {
            lock (shuttingDownMutex)
            {
                if (shuttingDown)
                {
                    return;
                }
                shuttingDown = true;
            }

            if (started)
            {
                Logger.LogInformation("ROS is shutting down.");
                started = false;
                _ok = false;

                SimTime.Terminate();
                RosOutAppender.Terminate();
                GlobalNodeHandle.shutdown();
                GlobalCallbackQueue.Disable();
                GlobalCallbackQueue.Clear();

                XmlRpcManager.Instance.Unbind("shutdown");
                Param.Terminate();

                TopicManager.Terminate();
                ServiceManager.Terminate();
                XmlRpcManager.Terminate();
                ConnectionManager.Terminate();
                PollManager.Terminate();

                ResetStaticMembers();
            }
        }


        private static void ResetStaticMembers()
        {
            globalCallbackQueue = null;
            initialized = false;
            _ok = false;
            timerManager = new TimerManager();
            started = false;
            _ok = false;
            shuttingDown = false;
            shutdownRequested = false;
        }

    }


    /// <summary>
    /// Options that can be passed to the ROS.init() function.
    /// </summary>
    [Flags]
    public enum InitOption
    {
        NosigintHandler = 1 << 0,
        AnonymousName = 1 << 1,
        NoRousout = 1 << 2
    }
}
