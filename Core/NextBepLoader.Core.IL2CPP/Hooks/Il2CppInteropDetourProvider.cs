using System;
using System.IO;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.Injection;
using MonoMod.Core;
using MonoMod.Core.Platforms;
using MonoMod.RuntimeDetour;

namespace NextBepLoader.Core.IL2CPP.Hooks;

internal class Il2CppInteropDetourProvider : IDetourProvider
{
    public IDetour Create<TDelegate>(nint original, TDelegate target) where TDelegate : Delegate
    {
        var p = Marshal.GetFunctionPointerForDelegate(target);
        return new Il2CppInteropDetour(Il2CppDetourFactory.CurrentFactory.CreateNativeDetour(original, p));
    }
}
