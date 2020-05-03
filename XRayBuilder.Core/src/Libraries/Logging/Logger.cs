using System;

namespace XRayBuilder.Core.Libraries.Logging
{
    public delegate void LogEventHandler(LogEventArgs e);

    public sealed class LogEventArgs : EventArgs
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
    }

    public sealed class Logger : ILogger
    {
        public event LogEventHandler LogEvent;

        public void Log(string message, LogLevel level = LogLevel.Auto)
        {
            LogEvent?.Invoke(new LogEventArgs
            {
                Message = message,
                Level = level
            });
        }
    }

    public sealed class ConsoleLogger : ILogger
    {
        public event LogEventHandler LogEvent;

        public void Log(string message, LogLevel level = LogLevel.Auto)
        {
            Console.WriteLine($"{level}: {message}");
        }
    }

    public interface ILogger
    {
        event LogEventHandler LogEvent;
        void Log(string message, LogLevel level = LogLevel.Auto);
    }

    public enum LogLevel
    {
        Auto = 1,
        Info,
        Warn,
        Error
    }
}
