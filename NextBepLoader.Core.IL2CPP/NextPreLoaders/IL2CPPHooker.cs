using System;
using System.Runtime.InteropServices;
using MonoMod.RuntimeDetour;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using UnityEngine;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class IL2CPPHook : BasePreLoader
{
    internal  NativeHook RuntimeInvokeDetour { get; set; }
    public override Type[] WaitLoadLoader => [typeof(IL2CPPPreLoader)];
    public Action<IL2CPPHook> OnActiveSceneChanged;

    public override void Start()
    {
        if (!TryGetHandle(out var il2CppHandle)) return;
        var runtimeInvokePtr = NativeLibrary.GetExport(il2CppHandle, "il2cpp_runtime_invoke");
        PreloaderLogger.Log.Log(LogLevel.Info, $"Runtime invoke pointer: 0x{runtimeInvokePtr.ToInt64():X}");
        RuntimeInvokeDetour = new NativeHook(runtimeInvokePtr, OnInvokeMethod, true);
        PreloaderLogger.Log.Log(LogLevel.Info, "Runtime invoke patched");
    }

    public virtual bool TryGetHandle(out nint handle)
    {
        if (NativeLibrary.TryLoad("GameAssembly", typeof(IL2CPPChainloader).Assembly, null, out handle)) return false;
        Logger.Log(LogLevel.Fatal,
                   "Could not locate Il2Cpp game assembly (GameAssembly.dll, UserAssembly.dll or libil2cpp.so). The game might be obfuscated or use a yet unsupported build of Unity.");
        return true;
    }
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr OrgInvokeDetourDelegate(IntPtr method, IntPtr obj, IntPtr parameters, IntPtr exc);
    
    private IntPtr OnInvokeMethod(OrgInvokeDetourDelegate originalInvoke,
                                  IntPtr method,
                                  IntPtr obj,
                                  IntPtr parameters,
                                  IntPtr exc)
    {
        var methodName = Marshal.PtrToStringAnsi(Il2CppInterop.Runtime.IL2CPP.il2cpp_method_get_name(method));

        var unhook = false;

        if (methodName == "Internal_ActiveSceneChanged")
            try
            {
                Logger.Sources.Add(new IL2CPPUnityLogSource());
                Application.CallLogCallback("Test call after applying unity logging hook", "", LogType.Assert, true);

                OnActiveSceneChanged.Invoke(this);
                unhook = true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Fatal, "Unable to execute IL2CPP chainloader");
                Logger.Log(LogLevel.Error, ex);
            }


        var result = originalInvoke(method, obj, parameters, exc);

        if (!unhook) return result;
        RuntimeInvokeDetour.Dispose();

        PreloaderLogger.Log.Log(LogLevel.Debug, "Runtime invoke unpatched");

        return result;
    }
}
