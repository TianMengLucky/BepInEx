using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader;

public static class PreloaderLogger
{
    public static ManualLogSource Log { get; } = Logger.CreateLogSource("Preloader");
}
