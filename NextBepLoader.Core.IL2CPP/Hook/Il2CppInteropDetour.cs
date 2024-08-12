using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.Injection;
using MonoMod.Core;
using MonoMod.Core.Platforms;

namespace NextBepLoader.Core.IL2CPP.Hook;

internal class Il2CppInteropDetour(ICoreNativeDetour detour) : IDetour
{
    public void Dispose() => detour.Dispose();

    public void Apply() => detour.Apply();

    public T GenerateTrampoline<T>() where T : Delegate
    {
        var stuff = PlatformTriple.Current.Architecture.ComputeDetourInfo(Target, Detour);
        Span<byte> buffer = stackalloc byte[stuff.Size];
        var b = PlatformTriple.Current.Architecture.GetDetourBytes(stuff, buffer, out var t);
        var n = PlatformTriple.Current.Architecture.AltEntryFactory.CreateAlternateEntrypoint(Target, b, out var o);
        return Marshal.GetDelegateForFunctionPointer<T>(n);
    }

    public nint Target => detour.Source;
    public nint Detour => detour.Target;

    public IntPtr OriginalTrampoline => detour.OrigEntrypoint;
}
