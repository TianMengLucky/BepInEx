using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NextBepLoader.Core.PreLoader.NextPreLoaders;


public class ResolvePreLoad : BasePreLoader
{
    public List<Assembly> ResolvedAssemblies { get; set; } = [];
    public override PreLoadPriority Priority => PreLoadPriority.VeryLast;

    public override void Start()
    {
        // Cecil 0.11 requires one to manually set up list of trusted assemblies for assembly resolving
        // The main BCL path
        AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Paths.ManagedPath);
        // The parent path -> .NET has some extra managed DLLs in there
        AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Path.GetDirectoryName(Paths.ManagedPath)!);
        AppDomain.CurrentDomain.AddCecilPlatformAssemblies(Paths.UnityBaseDirectory);

        AppDomain.CurrentDomain.AssemblyResolve += LocalResolve;
    }

    internal static Assembly? LocalResolve(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        var foundAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                     .FirstOrDefault(x => x.GetName().Name == assemblyName.Name);

        if (foundAssembly != null)
            return foundAssembly;


        if (
            Utility.TryResolveDllAssembly(assemblyName, Paths.CoreAssemblyPath, out foundAssembly)
          ||
            Utility.TryResolveDllAssembly(assemblyName, Paths.PluginPath, out foundAssembly)
          ||
            Utility.TryResolveDllAssembly(assemblyName, Paths.DependencyDirectory, out foundAssembly)
          ||
            Utility.TryResolveDllAssembly(assemblyName, Paths.IL2CPPInteropAssemblyDirectory, out foundAssembly)
          ||
            Utility.TryResolveDllAssembly(assemblyName, Paths.UnityBaseDirectory, out foundAssembly)
        )
            return foundAssembly;

        return null;
    }
}
