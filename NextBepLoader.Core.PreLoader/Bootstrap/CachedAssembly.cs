using System.Collections.Generic;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

/// <summary>
///     A cached assembly.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CachedAssembly<T> where T : ICacheableData
{
    /// <summary>
    ///     List of cached items inside the assembly.
    /// </summary>
    public List<T> CacheItems { get; set; }

    /// <summary>
    ///     Hash of the assembly. Used to verify that the assembly hasn't been changed.
    /// </summary>
    public string Hash { get; set; }
}
