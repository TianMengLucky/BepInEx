using System.Runtime.CompilerServices;

namespace NextBepLoader.Core.Logging.BepInExLogHandlers;

/// <inheritdoc />
[InterpolatedStringHandler]
public class BepInExDebugLogInterpolatedStringHandler : BepInExLogInterpolatedStringHandler
{
    /// <inheritdoc />
    public BepInExDebugLogInterpolatedStringHandler(int literalLength,
                                                    int formattedCount,
                                                    out bool isEnabled) : base(literalLength, formattedCount,
                                                                               LogLevel.Debug, out isEnabled) { }
}
