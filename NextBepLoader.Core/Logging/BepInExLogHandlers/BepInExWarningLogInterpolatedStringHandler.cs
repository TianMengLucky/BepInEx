using System.Runtime.CompilerServices;

namespace NextBepLoader.Core.Logging.BepInExLogHandlers;

/// <inheritdoc />
[InterpolatedStringHandler]
public class BepInExWarningLogInterpolatedStringHandler : BepInExLogInterpolatedStringHandler
{
    /// <inheritdoc />
    public BepInExWarningLogInterpolatedStringHandler(int literalLength,
                                                      int formattedCount,
                                                      out bool isEnabled) : base(literalLength, formattedCount,
             LogLevel.Warning, out isEnabled) { }
}
