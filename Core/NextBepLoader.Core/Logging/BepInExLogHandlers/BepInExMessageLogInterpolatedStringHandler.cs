namespace NextBepLoader.Core.Logging.BepInExLogHandlers;

/// <inheritdoc />
[InterpolatedStringHandler]
public class BepInExMessageLogInterpolatedStringHandler : BepInExLogInterpolatedStringHandler
{
    /// <inheritdoc />
    public BepInExMessageLogInterpolatedStringHandler(int literalLength,
                                                      int formattedCount,
                                                      out bool isEnabled) : base(literalLength, formattedCount,
             LogLevel.Message, out isEnabled) { }
}
