using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.Logging.BepInExLogHandlers;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.Logging;

/// <summary>
///     Handles pub-sub event marshalling across all log listeners and sources.
/// </summary>
public static class Logger
{
    private static readonly ManualLogSource MainLogSource;
    
    
    /// <summary>
    ///     Collection of all log source that output log events.
    /// </summary>
    public static readonly NextEventList<ILogSource> Sources = [];
    
    /// <summary>
    ///     Collection of all log listeners that receive log events.
    /// </summary>
    public static readonly NextEventList<ILogListener> Listeners = [];

    static Logger()
    {
        Sources.OnEvent += OnSourceEventList;
        Listeners.OnEvent += OnListenerEventList;
        MainLogSource = CreateLogSource(nameof(NextBepLoader));
    }

    private static bool OnSourceEventList(NextEventListEventArgs<ILogSource> eventArgs)
    {
        if (eventArgs.Type == ListEventType.Add)
            eventArgs.Value!.LogEvent += InternalLogEvent;

        if (eventArgs.Type == ListEventType.Clear)
            foreach (var source in eventArgs.List)
                source.LogEvent -= InternalLogEvent;

        if (eventArgs.Type == ListEventType.Remove)
            eventArgs.OnRemoved += args =>
            {
                args.Value!.LogEvent -= InternalLogEvent;
            };
        
        return NoNotify(eventArgs);
    }
    private static bool OnListenerEventList(NextEventListEventArgs<ILogListener> eventArgs)
    {
        if (eventArgs.Type == ListEventType.Add)
            ListenedLogLevels |= eventArgs.Value!.LogLevelFilter;

        if (eventArgs.Type == ListEventType.Clear)
            ListenedLogLevels = LogLevel.None;

        if (eventArgs.Type == ListEventType.Remove)
            eventArgs.OnRemoved += args =>
            {
                if (!args.Remove) return;
                ListenedLogLevels = LogLevel.None;
                foreach (var listener in eventArgs.List)
                    ListenedLogLevels |= listener.LogLevelFilter;
            };
        
        return NoNotify(eventArgs);
    }
    
    public static bool NoNotify<T>(NextEventListEventArgs<T> e) where T : class
    {
        return e.Type switch
        {
            ListEventType.Add      => true,
            ListEventType.Clear    => true,
            ListEventType.Contains => false,
            ListEventType.Remove   => false,
            var _                  => throw new ArgumentOutOfRangeException()
        };
    }
    
    /// <summary>
    ///     Log levels that are currently listened to by at least one listener.
    /// </summary>
    public static LogLevel ListenedLogLevels { get; private set; }

    internal static void InternalLogEvent(object sender, LogEventArgs eventArgs)
    {
        Task.Factory.StartNew(() =>
        {
            foreach (var listener in Listeners.Where(listener => (eventArgs.Level & listener.LogLevelFilter) !=
                                                                 LogLevel.None))
                listener.LogEvent(sender, eventArgs);
        });
    }

    /// <summary>
    ///     Logs an entry to the internal logger instance.
    /// </summary>
    /// <param name="level">The level of the entry.</param>
    /// <param name="data">The data of the entry.</param>
    internal static void Log(LogLevel level, object data) => MainLogSource.Log(level, data);

    /// <summary>
    ///     Logs an entry to the internal logger instance if any log listener wants the message.
    /// </summary>
    /// <param name="level">The level of the entry.</param>
    /// <param name="logHandler">Log handler to resolve log from.</param>
    internal static void Log(LogLevel level,
                             [InterpolatedStringHandlerArgument("level")]
                             BepInExLogInterpolatedStringHandler logHandler) =>
        MainLogSource.Log(level, logHandler);

    /// <summary>
    ///     Creates a new log source with a name and attaches it to <see cref="Sources" />.
    /// </summary>
    /// <param name="sourceName">Name of the log source to create.</param>
    /// <returns>An instance of <see cref="ManualLogSource" /> that allows to write logs.</returns>
    public static ManualLogSource CreateLogSource(string sourceName)
    {
        var source = new ManualLogSource(sourceName);
        Sources.Add(source);
        return source;
    }
}
