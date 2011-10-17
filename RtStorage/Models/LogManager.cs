using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RtStorage.Models
{
    public class LogManager
    {
        private static Logger _logger;

        public static void Initialize()
        {
            _logger = new Logger(new TraceLogger());
        }

        public static Logger GetLogger()
        {
            return _logger;
        }
    }

    public class Logger
    {
        private ILogger _logger;
        public Logger(ILogger logger)
        {
            _logger = logger;
        }

        public void Error(string message, Exception ex)
        {
            _logger.Write(LogLevel.Error, message);
        }
        public void Error(string message, params object[] args)
        {
            _logger.Write(LogLevel.Error, message);
        }
        public void Warn(string message, params object[] args)
        {
            _logger.Write(LogLevel.Warn, message);
        }
        public void Info(string message, params object[] args)
        {
            _logger.Write(LogLevel.Info, message);
        }
        public void Debug(string message, params object[] args)
        {
            _logger.Write(LogLevel.Debug, message);
        }
    }

    public enum LogLevel
    {
        Error,
        Warn,
        Info,
        Debug
    }

    public interface ILogger
    {
        void Write(LogLevel level, string message);
    }

    public class TraceLogger : ILogger
    {
        public void Write(LogLevel level, string message)
        {
            Trace.Write(message, level.ToString());
        }
    }

    public class QueueLogger : ILogger
    {
        private Queue<string> _queue = new Queue<string>(100);

        public void Write(LogLevel level, string message)
        {
            _queue.Enqueue(message);
        }
    }

}
