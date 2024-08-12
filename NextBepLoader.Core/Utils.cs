#nullable enable
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace NextBepLoader.Core;

public static class Utils
{
    public static readonly bool IsMono = Type.GetType("Mono.Runtime") != null ||
                                         Type.GetType("Mono.RuntimeStructs") != null ||
                                         PlatformDetection.Runtime.HasFlag(RuntimeKind.Mono);
    public static readonly bool IsCore = typeof(object).Assembly.GetName().Name == "System.Private.CoreLib" || PlatformDetection.Runtime.HasFlag(RuntimeKind.CoreCLR);
    public static T? GetExportAsDelegate<T>(this IntPtr s, string name) where T : class => s.GetExport(name).AsDelegate<T>();
    public static T? AsDelegate<T>(this IntPtr s) where T : class => Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;
    
    public static string PlatformPostFix => PlatformPostFixGet();

    private static string PlatformPostFixGet()
    {
        if (PlatformDetection.OS.Is(OSKind.OSX))
            return "dylib";

        if (PlatformDetection.OS.Is(OSKind.Posix))
            return "sp";
        
        return "dll";
    }
    
}
