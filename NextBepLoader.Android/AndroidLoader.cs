using NextBepLoader.Core;

namespace NextBepLoader.Android;

public sealed class AndroidLoader : LoaderBase
{
    public static AndroidLoader Instance { get; internal set; }
    public override LoaderPathBase Paths { get; set; }


    internal override bool TryStart()
    {
        return true;
    }
    
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
