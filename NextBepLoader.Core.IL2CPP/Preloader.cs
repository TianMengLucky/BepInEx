using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMod.Utils;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.RuntimeFixes;

namespace NextBepLoader.Core.IL2CPP;

public static class Preloader
{

    internal static ManualLogSource Log => PreloaderLogger.Log;

    // TODO: This is not needed, maybe remove? (Instance is saved in IL2CPPChainloader itself)
    private static IL2CPPChainloader Chainloader { get; set; }

    public static void Run()
    {
        try
        {
            HarmonyBackendFix.Initialize();
            UnityInfo.Initialize(Paths.ExecutablePath, Paths.GameDataPath);

            /*ConsoleManager.Initialize(false);*/


            /*Logger.Listeners.Add(PreloaderLog);

            if (ConsoleManager.ConsoleEnabled)
            {
                ConsoleManager.CreateConsole();
                Logger.Listeners.Add(new ConsoleLogListener());
            }

            RedirectStdErrFix.Apply();

            ChainloaderLogHelper.PrintLogInfo(Log);*/

            Logger.Log(LogLevel.Info, $"Running under Unity {UnityInfo.Version}");
            Logger.Log(LogLevel.Info, $"Runtime version: {Environment.Version}");
            Logger.Log(LogLevel.Info, $"Runtime information: {RuntimeInformation.FrameworkDescription}");
            Logger.Log(LogLevel.Info, $"OS information: {PlatformDetection.OS}");
            Logger.Log(LogLevel.Info, $"Game executable path: {Paths.ExecutablePath}");
            Logger.Log(LogLevel.Info, $"Interop assembly directory: {Il2CppInteropManager.IL2CPPInteropAssemblyPath}");
            Logger.Log(LogLevel.Info, $"BepInEx root path: {Paths.LoaderRootPath}");

            if (PlatformDetection.OS.Is(OSKind.Wine) && !Environment.Is64BitProcess)
                if (!NativeLibrary.TryGetExport(NativeLibrary.Load("ntdll"), "RtlRestoreContext", out var _))
                    Logger.Log(LogLevel.Warning,
                               "Your wine version doesn't support CoreCLR properly, expect crashes! Upgrade to wine 7.16 or higher.");
            

            Il2CppInteropManager.Initialize();
            


            Chainloader = new IL2CPPChainloader();

            Chainloader.Initialize();
        }
        catch (Exception ex)
        {
            Log.Log(LogLevel.Fatal, ex);

            throw;
        }
    }
    
}
