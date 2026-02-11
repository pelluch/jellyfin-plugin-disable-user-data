using Microsoft.Extensions.Logging;
using System;

namespace Jellyfin.Plugin.DisableUserData
{
    /// <summary>
    /// Logger wrapper that suppresses log output when DisableLogging is true.
    /// </summary>
    public class SuppressibleLogger<T> : ILogger<T>
    {
        private readonly ILogger<T> _innerLogger;
        /// <summary>
        /// If true, all log output is suppressed.
        /// </summary>
        public bool DisableLogging { get; set; } = false;

        public SuppressibleLogger(ILogger<T> innerLogger)
        {
            _innerLogger = innerLogger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _innerLogger.BeginScope(state);
        }

        /// <summary>
        /// Returns false if logging is suppressed, otherwise delegates to inner logger.
        /// </summary>
        public bool IsEnabled(LogLevel logLevel)
        {
            return !DisableLogging && _innerLogger.IsEnabled(logLevel);
        }

        /// <summary>
        /// Only logs if IsEnabled returns true.
        /// </summary>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                _innerLogger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}
