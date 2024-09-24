using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

/// <summary>
///     Provides methods for loading specified types from an assembly.
/// </summary>
public static class TypeLoader
{
    #region Config

    private static readonly ConfigEntry<bool> EnableAssemblyCache = ConfigFile.CoreConfig.Bind(
         "Caching", "EnableAssemblyCache",
         true,
         "Enable/disable assembly metadata cache\nEnabling this will speed up discovery of plugins and patchers by caching the metadata of all types BepInEx discovers.");

    #endregion
    

    /// <summary>
    ///     Looks up assemblies in the given directory and locates all types that can be loaded and collects their metadata.
    /// </summary>
    /// <typeparam name="T">The specific base type to search for.</typeparam>
    /// <param name="directory">The directory to search for assemblies.</param>
    /// <param name="typeSelector">A function to check if a type should be selected and to build the type metadata.</param>
    /// <param name="assemblyFilter">A filter function to quickly determine if the assembly can be loaded.</param>
    /// <param name="cacheName">The name of the cache to get cached types from.</param>
    /// <returns>
    ///     A dictionary of all assemblies in the directory and the list of type metadatas of types that match the
    ///     selector.
    /// </returns>
    public static Dictionary<string, List<T>> FindPluginTypes<T>(string directory,
                                                                 Func<TypeDefinition, string, T> typeSelector,
                                                                 Func<AssemblyDefinition, bool>? assemblyFilter =
                                                                     null,
                                                                 string? cacheName = null)
        where T : ICacheableData, new()
    {
        var result = new Dictionary<string, List<T>>();
        var hashes = new Dictionary<string, string>();
        Dictionary<string, CachedAssembly<T>>? cache = null;

        if (cacheName != null)
            cache = LoadAssemblyCache<T>(cacheName);

        foreach (var dll in Directory.GetFiles(Path.GetFullPath(directory), "*.dll", SearchOption.AllDirectories))
            try
            {
                var dllBytes = File.ReadAllBytes(dll);
                using (var dllMs = new MemoryStream(dllBytes))
                {
                    var hash = Utility.HashStream(dllMs);
                    hashes[dll] = hash;
                }
                if (cache != null && cache.TryGetValue(dll, out var cacheEntry)) 
                    if (hashes[dll] == cacheEntry.Hash)
                    {
                        result[dll] = cacheEntry.CacheItems;
                        continue;
                    }

                var assembly = AssemblyDefinition.FromBytes(dllBytes);
                Logger.Log(LogLevel.Debug, $"Examining '{dll}'");

                if (!assemblyFilter?.Invoke(assembly) ?? false)
                {
                    Logger.Log(LogLevel.Debug, $"NoFilter {dll}");
                    result[dll] = [];
                    continue;
                }

                var matches = assembly.ManifestModule?
                                      .GetAllTypes()
                                      .Select(t => typeSelector(t, dll))
                                      .Where(t => t != null)
                                      .ToList();
                result[dll] = matches ?? [];
                Logger.Log(LogLevel.Debug, $"Add {dll}");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e);
            }

        if (cacheName != null)
            SaveAssemblyCache(cacheName, result, hashes);

        return result;
    }

    /// <summary>
    ///     Loads an index of type metadatas from a cache.
    /// </summary>
    /// <param name="cacheName">Name of the cache</param>
    /// <typeparam name="T">Cacheable item</typeparam>
    /// <returns>
    ///     Cached type metadatas indexed by the path of the assembly that defines the type. If no cache is defined,
    ///     return null.
    /// </returns>
    public static Dictionary<string, CachedAssembly<T>>? LoadAssemblyCache<T>(string? cacheName)
        where T : ICacheableData, new()
    {
        if (!EnableAssemblyCache.Value)
            return null;

        var result = new Dictionary<string, CachedAssembly<T>>();
        try
        {
            var path = Path.Combine(Paths.CachePath, $"{cacheName}_typeloader.dat");
            if (!File.Exists(path))
                return null;

            using var br = new BinaryReader(File.OpenRead(path));
            var entriesCount = br.ReadInt32();

            for (var i = 0; i < entriesCount; i++)
            {
                var entryIdentifier = br.ReadString();
                var hash = br.ReadString();
                var itemsCount = br.ReadInt32();
                var items = new List<T>();

                for (var j = 0; j < itemsCount; j++)
                {
                    var entry = new T();
                    entry.Load(br);
                    items.Add(entry);
                }

                result[entryIdentifier] = new CachedAssembly<T> { Hash = hash, CacheItems = items };
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Warning,
                       $"Failed to load cache \"{cacheName}\"; skipping loading cache. Reason: {e.Message}.");
        }

        return result;
    }

    /// <summary>
    ///     Saves indexed type metadata into a cache.
    /// </summary>
    /// <param name="cacheName">Name of the cache</param>
    /// <param name="entries">List of plugin metadatas indexed by the path to the assembly that contains the types</param>
    /// <param name="hashes">Hash values that can be used for checking similarity between cached and live assembly</param>
    /// <typeparam name="T">Cacheable item</typeparam>
    public static void SaveAssemblyCache<T>(string? cacheName,
                                            Dictionary<string, List<T>> entries,
                                            Dictionary<string, string> hashes)
        where T : ICacheableData
    {
        if (!EnableAssemblyCache.Value)
            return;

        try
        {
            if (!Directory.Exists(Paths.CachePath))
                Directory.CreateDirectory(Paths.CachePath);

            var path = Path.Combine(Paths.CachePath, $"{cacheName}_typeloader.dat");

            using var bw = new BinaryWriter(File.OpenWrite(path));
            bw.Write(entries.Count);

            foreach (var kv in entries)
            {
                bw.Write(kv.Key);
                bw.Write(hashes.GetValueOrDefault(kv.Key, ""));
                bw.Write(kv.Value.Count);

                foreach (var item in kv.Value)
                    item.Save(bw);
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Warning,
                       $"Failed to save cache \"{cacheName}\"; skipping saving cache. Reason: {e.Message}.");
        }
    }
}
