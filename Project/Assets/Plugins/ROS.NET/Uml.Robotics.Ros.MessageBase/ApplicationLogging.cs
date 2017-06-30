//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Console;
//using DummyLogging;
//using UnityEngine;
/*
namespace Uml.Robotics.Ros
{
    public static class ApplicationLogging
    {
		UnityEngine.Debug debug;
        private static ILoggerFactory _loggerFactory;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if(_loggerFactory == null)
                {
                    _loggerFactory = new LoggerFactory();
                    _loggerFactory.AddProvider(
                        new ConsoleLoggerProvider(
                            (string text, LogLevel logLevel) => { return logLevel > LogLevel.Debug;}, true)
                    );
                }
                return _loggerFactory;
            }
            set
            {
                _loggerFactory = value;
            }

        }

		public static ILogger CreateLogger<T> ()
		{
			return new Logger ( Debug.logger );
//			return LoggerFactory.CreateLogger<T> ();
		}

		public static ILogger CreateLogger (string category)
		{
			return new Logger ( Debug.logger );
//			return LoggerFactory.CreateLogger ( category );
		}
//        public static ILogger CreateLogger<T>() =>
//            LoggerFactory.CreateLogger<T>();

//        public static ILogger CreateLogger(string category) =>
//            LoggerFactory.CreateLogger(category);
    }
}
*/