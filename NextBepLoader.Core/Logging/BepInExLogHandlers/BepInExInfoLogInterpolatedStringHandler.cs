using System.Runtime.CompilerServices;

namespace NextBepLoader.Core.Logging.BepInExLogHandlers;

/// <inheritdoc />
[InterpolatedStringHandler]
public class BepInExInfoLogInterpolatedStringHandler : BepInExLogInterpolatedStringHandler
{
    /// <inheritdoc />
    public BepInExInfoLogInterpolatedStringHandler(int literalLength,
                                                   int formattedCount,
                                                   out bool isEnabled) : base(literalLength, formattedCount,
                                                                              LogLevel.Info, out isEnabled) { }
}
