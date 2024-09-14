using System;
using MonoMod.Core;
using MonoMod.RuntimeDetour;
using NextBepLoader.Core.IL2CPP.Hook.Dobby;

namespace NextBepLoader.Core.IL2CPP.Hook;

public class Il2CppDetourFactory : IDetourFactory
{
    
    public ICoreDetour CreateDetour(CreateDetourRequest request) =>
        DetourContext.CurrentFactory?.CreateDetour(request) ?? throw new NullReferenceException();

    public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request)
    {
        return CreateDefault(request.Source, request.Target);
    }

    private static ICoreNativeDetour CreateDefault(nint original, IntPtr target) =>
        // TODO: check and provide an OS accurate provider
        new DobbyDetour(original, target);
    
}
