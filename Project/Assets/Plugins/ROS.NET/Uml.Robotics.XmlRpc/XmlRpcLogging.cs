//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Console;

namespace Uml.Robotics.XmlRpc
{
    public static class XmlRpcLogging
    {
//        private static ILoggerFactory _loggerFactory;

        public static bool Initialized
        {
            get
            {
                lock (typeof(XmlRpcLogging))
                {
					return true;
//                    return _loggerFactory != null;
                }
            }
        }

/*        public static ILoggerFactory LoggerFactory
        {
            get
            {
                lock (typeof(XmlRpcLogging))
                {
                    if (_loggerFactory == null)
                    {
                        _loggerFactory = new LoggerFactory();
                        _loggerFactory.AddProvider(
                            new ConsoleLoggerProvider(
                                (string text, LogLevel logLevel) => { return logLevel > LogLevel.Debug; }, true)
                        );
                    }
                    return _loggerFactory;
                }
            }
            set
            {
                lock (typeof(XmlRpcLogging))
                {
                    _loggerFactory = value;
                }
            }
        }*/

/*		public static ILogger CreateLogger<T>()
		{
			return LoggerFactory.CreateLogger<T>();
		}*/

/*        public static ILogger CreateLogger(string category)
		{
			return LoggerFactory.CreateLogger(category);
		}*/
    }
}
