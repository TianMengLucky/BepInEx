using System;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftLogging = Microsoft.Extensions.Logging;

using Microsoft.Extensions.Logging;

namespace NextBepLoader.Core.Logging;

public class NextLoggerProvider : ILoggerProvider
{
    public void Dispose() => throw new NotImplementedException();

    public MicrosoftLogging.ILogger CreateLogger(string categoryName) => throw new NotImplementedException();
    
    public class NextLogger : MicrosoftLogging.ILogger
    {
        public void Log<TState>(MicrosoftLogging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => throw new NotImplementedException();

        public bool IsEnabled(MicrosoftLogging.LogLevel logLevel) => throw new NotImplementedException();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
    }
}

public static class NextLoggerExtension
{
    public static IServiceCollection AddNextLogger(this IServiceCollection services)
    {
        services.AddLogging(n => n.AddProvider(new NextLoggerProvider()));
        return services;
    }
}
