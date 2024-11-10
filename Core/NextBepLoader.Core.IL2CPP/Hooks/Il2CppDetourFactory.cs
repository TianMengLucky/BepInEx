using System;
using System.IO;
using MonoMod.Core;
using MonoMod.Core.Platforms;
using NextBepLoader.Core.IL2CPP.Hooks.Dobby;

namespace NextBepLoader.Core.IL2CPP.Hooks;

public class Il2CppDetourFactory : IDetourFactory
{

    public ICoreDetour CreateDetour(CreateDetourRequest request) =>
        DetourFactory.Current.CreateDetour(request.Source, request.Target, request.ApplyByDefault) ?? throw new NullReferenceException();

    public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request)
    {
        if (File.Exists(Path.Combine(Paths.CoreDirectory, "dobby.dll")))
        {
            return new DobbyNativeDetour(request.Source, request.Target);
        }
        return DetourFactory.Current.CreateNativeDetour(request.Source, request.Target, request.ApplyByDefault) ?? throw new NullReferenceException();
    }
}
