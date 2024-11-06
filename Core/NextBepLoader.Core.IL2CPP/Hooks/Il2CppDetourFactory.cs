using System;
using MonoMod.Core;
using NextBepLoader.Core.IL2CPP.Hooks.Dobby;

namespace NextBepLoader.Core.IL2CPP.Hooks;

public class Il2CppDetourFactory : IDetourFactory
{

    public ICoreDetour CreateDetour(CreateDetourRequest request) =>
        DetourFactory.Current.CreateDetour(request.Source, request.Target, request.ApplyByDefault) ?? throw new NullReferenceException();

    public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request)
    {
        return CreateDefault(request.Source, request.Target);
    }

    private static ICoreNativeDetour CreateDefault(nint original, IntPtr target) =>
        // TODO: check and provide an OS accurate provider
        new DobbyNativeDetour(original, target);
    
}
