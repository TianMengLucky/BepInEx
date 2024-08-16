using NextBepLoader.Core;
using NextBepLoader.Core.Contract;

namespace NextBepLoader.Android;

public sealed class AndroidLoader : LoaderBase
{
    public static AndroidLoader Instance { get; internal set; }
    public override LoaderPathBase Paths { get; set; }
    public override LoaderPlatformType LoaderType => LoaderPlatformType.Android;


    internal override bool TryStart() => true;

    internal static bool TryCreateLoader()
    {
        try
        {
            Instance = new AndroidLoader();
        }
        catch
        {
            // ignored
            return false;
        }

        return true;
    }
}
