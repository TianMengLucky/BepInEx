using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.Logging.DefaultSource;
using NextBepLoader.Core.Logging.Interface;

namespace NextBepLoader.Core.Logging.Extensions;

public static class NextLoggerExtension
{
    public static IServiceCollection AddNextLogger(this IServiceCollection services)
    {
        services.AddLogging(n => n.AddProvider(new NextLoggerProvider()));
        return services;
    }

    public static IServiceCollection AddTraceLogSource(this IServiceCollection services)
    {
        services.AddSingleton(TraceLogSource.CreateListener());
        TraceLogSource.CreateSource().Register();
        return services;
    }

    public static ILogListener Register(this ILogListener logListener)
    {
        Logger.Listeners.Add(logListener);
        return logListener;
    }

    public static ILogSource Register(this ILogSource logSource)
    {
        Logger.Sources.Add(logSource);
        return logSource;
    }
}
