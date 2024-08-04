using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.Injection;
using MonoMod.Core;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace BepInEx.Unity.IL2CPP.Hook;

internal class Il2CppInteropDetourProvider : IDetourProvider
{
    public IDetour Create<TDelegate>(nint original, TDelegate target) where TDelegate : Delegate
    {
        if (DetourContext.Current == null)
            DetourContext.SetGlobalContext(new DetourFactoryContext(new Il2CppDetourFactory()));
        
        var p = Marshal.GetFunctionPointerForDelegate(target);
        return new Il2CppInteropDetour(DetourContext.Current!.Factory!.CreateNativeDetour(original, p));
    }
}
