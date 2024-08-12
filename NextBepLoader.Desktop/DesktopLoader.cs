using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase
{
    public static DesktopLoader Instance { get; private set; }
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();
    internal IServiceCollection Collection { get; set; }


    internal override bool TryStart()
    {
        Collection = new ServiceCollection();
        LoaderVersion = new Version();
        
        PlatformUtils.SetDesktopPlatformVersion();

        Collection.AddSingleton<IPreLoaderManager, DesktopPreLoadManager>();
        
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
