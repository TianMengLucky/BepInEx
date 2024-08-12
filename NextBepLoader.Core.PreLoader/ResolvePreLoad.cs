using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NextBepLoader.Core.PreLoader;

public class ResolvePreLoad : BasePreLoader
{
    public override void Start()
    {
        // Cecil 0.11 requires one to manually set up list of trusted assemblies for assembly resolving
        // The main BCL path
        AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Paths.ManagedPath);
        // The parent path -> .NET has some extra managed DLLs in there
        AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Path.GetDirectoryName(Paths.ManagedPath)!);
        
        AppDomain.CurrentDomain.AssemblyResolve += LocalResolve;
    }
    
    internal static Assembly? LocalResolve(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        var foundAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                     .FirstOrDefault(x => x.GetName().Name == assemblyName.Name);

        if (foundAssembly != null)
            return foundAssembly;

        if (Utility.TryResolveDllAssembly(assemblyName, Paths.BepInExAssemblyDirectory, out foundAssembly)
         || Utility.TryResolveDllAssembly(assemblyName, Paths.PatcherPluginPath, out foundAssembly)
         || Utility.TryResolveDllAssembly(assemblyName, Paths.PluginPath, out foundAssembly))
            return foundAssembly;

        return null;
    }
}
