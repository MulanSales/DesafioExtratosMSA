using System;
using Microsoft.Extensions.Logging;

namespace Releases.Tests.Wrappers
{
    public class LoggerWrapper<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            return;
        }

        public void LogInformation(string message) 
        {
            return;
        }
    }
}