using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

/// <summary>
///     Provides methods for loading specified types from an assembly.
/// </summary>
public class FastTypeFinder<T>(string directory, FastCaches<T>? caches = null) where T : ICacheableData, new()
{

    public DirectoryInfo FindDirectory { get; } = new(directory);
    public FastCaches<T>? Caches { get; set; } = caches;

    public Dictionary<string, List<T>> FindFormTypeLoader(Func<TypeDefinition, string, T> typeSelector,
                                                          Func<AssemblyDefinition, bool>? assemblyFilter)
    {
        var result = new Dictionary<string, List<T>>();
        var loader = new DotNetLoader
        { 
            AssemblyFilter = assemblyFilter    
        };
        
        foreach (var (assembly, types) in loader.AddAssembliesFormDirector(FindDirectory).LoadTypes()._LoadTypes)
        {
            if (assembly.ManifestModule == null)
            {
                Logger.LogWarning($"{assembly.Name} has no ManifestModule");
                continue;
            }
            
            var path = assembly.ManifestModule.FilePath!;
            try
            {
                using var dllStream = new MemoryStream();
                assembly.ManifestModule.Write(dllStream);
                Caches?.OnReadAssembly(path, dllStream);
                
                Logger.LogDebug($"Examining '{path}'");

                if (Caches?.TryGet(path, out var items) ?? false)
                    if (items.Count != 0)
                    {
                        result[path] = items;
                        continue;
                    }

                var matches = types.Select(t => typeSelector(t, path)).ToList();
                result[path] = matches;
                Caches?.OnAddAssembly(path, matches);
                Logger.LogDebug($"Add {path}");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        return result;
    }
    
    
    /// <summary>
    ///     Looks up assemblies in the given directory and locates all types that can be loaded and collects their metadata.
    /// </summary>
    /// <typeparam name="T">The specific base type to search for.</typeparam>
    /// <param name="typeSelector">A function to check if a type should be selected and to build the type metadata.</param>
    /// <param name="assemblyFilter">A filter function to quickly determine if the assembly can be loaded.</param>
    /// <returns>
    ///     A dictionary of all assemblies in the directory and the list of type metadatas of types that match the
    ///     selector.
    /// </returns>
    public Dictionary<string, List<T>> FindPluginTypes(Func<TypeDefinition, string, T> typeSelector, Func<AssemblyDefinition, bool>? assemblyFilter) 
    {
        var result = new Dictionary<string, List<T>>();
        foreach (var dll in FindDirectory.GetFiles("*.dll", SearchOption.AllDirectories))
            try
            {
                var dllName = dll.FullName;
                var dllBytes = File.ReadAllBytes(dllName);
                using (var dllMs = new MemoryStream(dllBytes))
                    Caches?.OnReadAssembly(dllName, dllMs);

                var assembly = AssemblyDefinition.FromBytes(dllBytes);
                Logger.LogDebug($"Examining '{dllName}'");

                if (Caches?.TryGet(dllName, out var items) ?? false)
                    if (items.Count != 0)
                    {
                        result[dllName] = items;
                        continue;
                    }
                
                
                if (!assemblyFilter?.Invoke(assembly) ?? false)
                {
                    Logger.Log(LogLevel.Debug, $"NoFilter {dll}");
                    result[dllName] = [];
                    continue;
                }

                var matches = assembly.ManifestModule?
                                      .GetAllTypes()
                                      .Select(t => typeSelector(t, dllName))
                                      .ToList();
                result[dllName] = matches ?? [];
                Caches?.OnAddAssembly(dllName, matches ?? []);
                Logger.LogDebug($"Add {dll}");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

        return result;
    }
}
