using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NextBepLoader.Core.Logging;

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
        var source = TraceLogSource.CreateSource();
        if (!Logger.Sources.Contains(source))
            Logger.Sources.Add(source);
        return services;
    }
}
