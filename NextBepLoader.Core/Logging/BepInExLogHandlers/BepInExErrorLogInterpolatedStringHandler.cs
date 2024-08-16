namespace NextBepLoader.Core.Logging.BepInExLogHandlers;

/// <inheritdoc />
[InterpolatedStringHandler]
public class BepInExErrorLogInterpolatedStringHandler : BepInExLogInterpolatedStringHandler
{
    /// <inheritdoc />
    public BepInExErrorLogInterpolatedStringHandler(int literalLength,
                                                    int formattedCount,
                                                    out bool isEnabled) : base(literalLength, formattedCount,
                                                                               LogLevel.Error, out isEnabled) { }
}
