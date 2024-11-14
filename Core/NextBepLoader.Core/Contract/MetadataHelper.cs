using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using NextBepLoader.Core.Contract.Attributes;

namespace NextBepLoader.Core.Contract;

public static class MetadataHelper
{
    internal static IEnumerable<CustomAttribute> GetCustomAttributes<T>(TypeDefinition td, bool inherit)
        where T : Attribute
    {
        var result = new List<CustomAttribute>();
        var type = typeof(T);
        var currentType = td;

        do
        {
            result.AddRange(currentType?.CustomAttributes.Where(ca => ca.Constructor!.FullName == type.FullName)!);
            currentType = currentType?.BaseType?.Resolve();
        } while (inherit && currentType?.FullName != "System.Object");


        return result;
    }

 
    

    /// <summary>
    ///     Gets the specified attributes of a type, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="pluginType">The plugin type.</param>
    /// <returns>The attributes of the type, if existing.</returns>
    public static T[] GetAttributes<T>(Type pluginType) where T : Attribute =>
        (T[])pluginType.GetCustomAttributes(typeof(T), true);

    /// <summary>
    ///     Gets the specified attributes of an assembly, if they exist.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <returns>The attributes of the type, if existing.</returns>
    public static T[] GetAttributes<T>(Assembly assembly) where T : Attribute =>
        (T[])assembly.GetCustomAttributes(typeof(T), true);

    /// <summary>
    ///     Gets the specified attributes of an instance, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="plugin">The plugin instance.</param>
    /// <returns>The attributes of the instance, if existing.</returns>
    public static IEnumerable<T> GetAttributes<T>(object plugin) where T : Attribute =>
        GetAttributes<T>(plugin.GetType());

    /// <summary>
    ///     Gets the specified attributes of a reflection metadata type, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="member">The reflection metadata instance.</param>
    /// <returns>The attributes of the instance, if existing.</returns>
    public static T[] GetAttributes<T>(MemberInfo member) where T : Attribute =>
        (T[])member.GetCustomAttributes(typeof(T), true);

    /// <summary>
    ///     Retrieves the dependencies of the specified plugin type.
    /// </summary>
    /// <param name="plugin">The plugin type.</param>
    /// <returns>A list of all plugin types that the specified plugin type depends upon.</returns>
    public static IEnumerable<PluginDependency> GetDependencies(this Type plugin) =>
        plugin.GetCustomAttributes(typeof(PluginDependency), true).Cast<PluginDependency>();
    
    
    public static IEnumerable<PluginCompatibility> FromCecilType(this TypeDefinition td)
    {
        var attrs = GetCustomAttributes<PluginCompatibility>(td, true);
        return attrs.Select(customAttribute =>
        {
            var dependencyGuid = (string)customAttribute.Signature!.NamedArguments[0].Argument.Element!;
            return new PluginCompatibility(dependencyGuid);
        }).ToList();
    }
    
    public static void SetMedata(this PluginMetadata metadata, Type type)
    {
        if (string.IsNullOrEmpty(metadata.Name))
            metadata.Name = type.Name;
        
        var processes = type.GetCustomAttributes<PluginProcess>(false).ToList();
        if (processes.Any())
            metadata.Processes = processes;
        
        var dependencies = type.GetCustomAttributes<PluginDependency>(false).ToList();
        if (dependencies.Any())
            metadata.Dependencies = dependencies;
        
        var compatibilities = type.GetCustomAttributes<PluginCompatibility>(false).ToList();
        if (compatibilities.Any())
            metadata.Compatibilities = compatibilities;
    }
}
