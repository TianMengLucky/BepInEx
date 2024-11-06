using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.PreLoader.NextPreLoaders;


public class ResolvePreLoad : BasePreLoader
{
    public List<Assembly> ResolvedAssemblies { get; set; } = [];
    public static readonly IReadOnlyList<string> TargetDirectors = 
        [
            Paths.CoreAssemblyPath,
            Paths.IL2CPPInteropAssemblyDirectory,
            Paths.UnityBaseDirectory,
            Paths.DependencyDirectory,
            Paths.PluginPath
        ];
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

        foreach (var dir in TargetDirectors)
        {
            if (Utility.TryResolveDllAssembly(assemblyName, dir, out var assembly))
            {
                return assembly;
            }
        }

        return null;
    }
}
