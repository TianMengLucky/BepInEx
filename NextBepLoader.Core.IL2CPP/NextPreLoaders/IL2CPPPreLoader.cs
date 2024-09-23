using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppSystem.Text;
using Microsoft.Extensions.Logging;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.IL2CPP.Hooks;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class IL2CPPPreLoader(INextBepEnv env, ILogger<IL2CPPPreLoader> logger, UnityInfo unityInfo) : BasePreLoader
{
    public override PreLoadPriority Priority => PreLoadPriority.VeryLast;
    public IL2CPPCheckEventArg _IL2CPPCheckEventArg;
    
    private static readonly ConfigEntry<bool> UpdateInteropAssemblies =
        ConfigFile.CoreConfig.Bind("IL2CPP",
                                   "UpdateInteropAssemblies",
                                   true,
                                   new StringBuilder()
                                       .AppendLine("Whether to run Il2CppInterop automatically to generate Il2Cpp support assemblies when they are outdated.")
                                       .AppendLine("If disabled assemblies in `BepInEx/interop` won't be updated between game or BepInEx updates!")
                                       .ToString());

    public override void PreLoad(PreLoadEventArg arg)
    {
        logger.LogInformation("Running under Unity {version}", unityInfo.GetVersion());
        logger.LogInformation("Runtime version: {Version}", Environment.Version);
        logger.LogInformation("Runtime information: {info}", RuntimeInformation.FrameworkDescription);
        logger.LogInformation("OS information: {os}", PlatformDetection.OS);
        logger.LogInformation("Game executable path: {path}", Paths.ExecutablePath);
        logger.LogInformation("Interop assembly directory: {dir}", Paths.IL2CPPInteropAssemblyDirectory);
        logger.LogInformation("Loader root path: {path}", Paths.LoaderRootPath);
        logger.LogInformation("Loader Core Assembly path: {path}", Paths.CoreAssemblyPath);
        logger.LogInformation("Loader Type {type}", LoaderInstance.Current.LoaderType);

        if (PlatformDetection.OS.Is(OSKind.Wine) && !Environment.Is64BitProcess) 
            if (!NativeLibrary.TryGetExport(NativeLibrary.Load("ntdll"), "RtlRestoreContext", out var _)) 
                logger.LogWarning("Your wine version doesn't support CoreCLR properly, expect crashes! Upgrade to wine 7.16 or higher.");
        
        _IL2CPPCheckEventArg = env.GetOrCreateEventArgs<IL2CPPCheckEventArg>();
        _IL2CPPCheckEventArg.UpdateIL2CPPInteropAssembly = UpdateInteropAssemblies.Value;

        env.RegisterSystemEnv("IL2CPP_INTEROP_DATABASES_LOCATION", Paths.IL2CPPInteropAssemblyDirectory);
    }

    public override void Start()
    {
        NativeLibrary.SetDllImportResolver(typeof(Il2CppInterop.Runtime.IL2CPP).Assembly, DllImportResolver);
        DetourContext.SetGlobalContext(new DetourFactoryContext(new Il2CppDetourFactory()));
    }
    
    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName == "GameAssembly" ? NativeLibrary.Load(Paths.GameAssemblyPath, assembly, searchPath) : IntPtr.Zero;
    }
}
