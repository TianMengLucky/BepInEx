using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP.Hook.Dobby;
using Il2CppInterop.Runtime.Injection;
using MonoMod.Utils;

namespace BepInEx.Unity.IL2CPP.Hook;

public interface INativeDetour : IDetour
{
    private static readonly ConfigEntry<DetourProvider> DetourProviderType = ConfigFile.CoreConfig.Bind(
         "Detours", "DetourProviderType",
         DetourProvider.Default,
         "The native provider to use for managed detours"
        );

    public nint OriginalMethodPtr { get; }
    public nint DetourMethodPtr { get; }
    public nint TrampolinePtr { get; }

    private static INativeDetour CreateDefault<T>(nint original, T target) where T : Delegate =>
        // TODO: check and provide an OS accurate provider
        new DobbyDetour(original, target);

    public static INativeDetour Create<T>(nint original, T target) where T : Delegate
    {
        var detour = DetourProviderType.Value switch
        {
            DetourProvider.Dobby    => new DobbyDetour(original, target),
            var _                   => CreateDefault(original, target)
        };
        return !PlatformDetection.Runtime.HasFlag(RuntimeKind.Mono) ? new CacheDetourWrapper(detour, target) : detour;
    }

    public static INativeDetour CreateAndApply<T>(nint from, T to, out T original)
        where T : Delegate
    {
        var detour = Create(from, to);
        original = detour.GenerateTrampoline<T>();
        detour.Apply();

        return detour;
    }

    // Workaround for CoreCLR collecting all delegates
    private class CacheDetourWrapper : INativeDetour
    {
        private readonly INativeDetour wrapped;

        private readonly List<object> cache = new();

        public CacheDetourWrapper(INativeDetour wrapped, Delegate target)
        {
            this.wrapped = wrapped;
            cache.Add(target);
        }

        public void Dispose()
        {
            wrapped.Dispose();
            cache.Clear();
        }

        public void Apply() => wrapped.Apply();
        
        public T GenerateTrampoline<T>() where T : Delegate
        {
            var trampoline = wrapped.GenerateTrampoline<T>();
            cache.Add(trampoline);
            return trampoline;
        }

        public nint Target { get; }
        public nint Detour { get; }
        public nint OriginalTrampoline { get; }
        

        public nint OriginalMethodPtr => wrapped.OriginalMethodPtr;

        public nint DetourMethodPtr => wrapped.DetourMethodPtr;

        public nint TrampolinePtr => wrapped.TrampolinePtr;
    }

    internal enum DetourProvider
    {
        Default,
        Dobby
    }
}
