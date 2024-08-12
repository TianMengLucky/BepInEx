using System;
using MonoMod.Core;
using MonoMod.RuntimeDetour;
using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.IL2CPP.Hook.Dobby;
using NextBepLoader.Core.IL2CPP.Hook.Funchook;

namespace NextBepLoader.Core.IL2CPP.Hook;

public class Il2CppDetourFactory : IDetourFactory
{
    
    private static readonly ConfigEntry<DetourProvider> DetourProviderType = ConfigFile.CoreConfig.Bind(
         "Detours", "DetourProviderType",
         DetourProvider.Default,
         "The native provider to use for managed detours"
        );

    private static ICoreNativeDetour CreateDefault(nint original, IntPtr target) =>
        // TODO: check and provide an OS accurate provider
        new DobbyDetour(original, target);
    
    public ICoreDetour CreateDetour(CreateDetourRequest request)
    {
        return DetourContext.CurrentFactory?.CreateDetour(request) ?? throw new NullReferenceException();
    }

    public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request)
    {
        var detour = DetourProviderType.Value switch
        {
            DetourProvider.Dobby    => new DobbyDetour(request.Source, request.Target),
            DetourProvider.Funchook => new FunchookDetour(request.Source, request.Target),
            var _                   => CreateDefault(request.Source, request.Target)
        };

        return detour;
    }

    internal enum DetourProvider
    {
        Default,
        Dobby,
        Funchook
    }
}
