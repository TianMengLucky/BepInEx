using System.IO;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

/// <summary>
///     A cacheable metadata item. cache data
/// </summary>
public interface ICacheableData
{
    /// <summary>
    ///     Serialize the object into a binary format.
    /// </summary>
    /// <param name="bw"></param>
    void Save(BinaryWriter bw);

    /// <summary>
    ///     Loads the object from binary format.
    /// </summary>
    /// <param name="br"></param>
    void Load(BinaryReader br);
}
