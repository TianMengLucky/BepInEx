using System.Reflection;
using NextBepLoader.Core;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase
{
    public static DesktopLoader Instance { get; private set; }
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();


    internal override bool TryStart()
    {
        LoaderVersion = new Version();
        return true;
    }
    
    internal static bool TryCreateLoader()
    {
        try
        {
            Instance = new DesktopLoader();
        }
        catch
        {
            // ignored
            return false;
        }

        return true;
    }
}
