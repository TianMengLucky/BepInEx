using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class IL2CPPPreLoader : BasePreLoader
{
    public override PreLoadPriority Priority => PreLoadPriority.VeryLast;

    public override void Start()
    {
        NativeLibrary.SetDllImportResolver(typeof(Il2CppInterop.Runtime.IL2CPP).Assembly, DllImportResolver);
    }
    
    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName == "GameAssembly" ? NativeLibrary.Load(Il2CppInteropManager.GameAssemblyPath, assembly, searchPath) : IntPtr.Zero;
    }
}
