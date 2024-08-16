using System;
using System.Runtime.InteropServices;
using MonoMod.Utils;

namespace NextBepLoader.Core;

public static class Utils
{
    public static readonly bool IsMono = PlatformDetection.Runtime == RuntimeKind.Mono;

    public static readonly bool IsCore = PlatformDetection.Runtime == RuntimeKind.CoreCLR;

    public static string PlatformPostFix => PlatformPostFixGet();
    public static string PlatformGameAssemblyName => PlatformGameAssemblyNameGet();

    public static T? GetExportAsDelegate<T>(this IntPtr s, string name) where T : class =>
        s.GetExport(name).AsDelegate<T>();

    public static T? AsDelegate<T>(this IntPtr s) where T : class =>
        Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;
    
    private static string PlatformGameAssemblyNameGet()
    {
        if (PlatformDetection.OS.Is(OSKind.Android))
            return "libil2cpp";
        
        return "GameAssembly";
    }

    private static string PlatformPostFixGet()
    {
        if (PlatformDetection.OS.Is(OSKind.Android))
            return "so";
        
        if (PlatformDetection.OS.Is(OSKind.OSX))
            return "dylib";

        if (PlatformDetection.OS.Is(OSKind.Posix))
            return "sp";

        return "dll";
    }
}
