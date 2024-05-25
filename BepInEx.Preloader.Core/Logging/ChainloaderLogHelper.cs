using System.Linq;
using System.Text;
using BepInEx.Core.Logging;
using BepInEx.Logging;
using MonoMod.Utils;

namespace BepInEx.Preloader.Core.Logging;

public static class ChainloaderLogHelper
{
    public static void PrintLogInfo(ManualLogSource log)
    {
        var bepinVersion = Paths.BepInExVersion;
        var versionMini = new SemanticVersioning.Version(bepinVersion.Major, bepinVersion.Minor, bepinVersion.Patch,
                                                         bepinVersion.PreRelease);
        var consoleTitle = $"BepInEx {versionMini} - {Paths.ProcessName}";
        log.Log(LogLevel.Message, consoleTitle);

        if (!string.IsNullOrEmpty(bepinVersion.Build))
            log.Log(LogLevel.Message, $"Built from commit {bepinVersion.Build}");

        Logger.Log(LogLevel.Info, $"System platform: {GetPlatformString()}");
        Logger.Log(LogLevel.Info,
                   $"Process bitness: {(PlatformDetection.Architecture.Has(ArchitectureKind.Bits64) ? "64-bit (x64)" : "32-bit (x86)")}");
    }

    private static string GetPlatformString()
    {
        var builder = new StringBuilder();

        switch (PlatformDetection.OS)
        {
            case OSKind.Android:
                builder.Append(" Android");
                break;
        }

        if (PlatformDetection.Architecture.Has(ArchitectureKind.Arm))
        {
            builder.Append(" ARM");
        }
        
        builder.Append(PlatformDetection.Architecture.Has(ArchitectureKind.Bits64) ? " 64-bit" : " 32-bit");

        return builder.ToString();
    }

    public static void RewritePreloaderLogs()
    {
        if (PreloaderConsoleListener.LogEvents == null || PreloaderConsoleListener.LogEvents.Count == 0)
            return;

        // Temporarily disable the console log listener (if there is one from preloader) as we replay the preloader logs
        var logListener = Logger.Listeners.FirstOrDefault(logger => logger is ConsoleLogListener);

        if (logListener != null)
            Logger.Listeners.Remove(logListener);

        foreach (var preloaderLogEvent in PreloaderConsoleListener.LogEvents)
            Logger.InternalLogEvent(PreloaderLogger.Log, preloaderLogEvent);

        if (logListener != null)
            Logger.Listeners.Add(logListener);
    }
}
