using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace NextBepLoader.Core.Contract.Attributes;

/// <summary>
///     This attribute specifies which processes this plugin should be run for. Not specifying this attribute will load the
///     plugin under every process.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class BepInProcess : Attribute
{
    /// <param name="ProcessName">The name of the process that this plugin will run under.</param>
    public BepInProcess(string ProcessName)
    {
        this.ProcessName = ProcessName;
    }

    /// <summary>
    ///     The name of the process that this plugin will run under.
    /// </summary>
    public string ProcessName { get; protected set; }

    internal static List<BepInProcess> FromCecilType(TypeDefinition td)
    {
        var attrs = MetadataHelper.GetCustomAttributes<BepInProcess>(td, true);
        return attrs.Select(customAttribute =>
                                new BepInProcess((string)customAttribute.ConstructorArguments[0].Value)).ToList();
    }
}
