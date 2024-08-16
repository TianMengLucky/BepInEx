using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.NextPreLoaders;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase
{
    private List<BasePreLoader> Loaders = [];
    public static DesktopLoader Instance { get; private set; }
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();

    public override LoaderPlatformType LoaderType => LoaderPlatformType.Desktop;
    internal IServiceCollection Collection { get; set; }
    public new Action<IServiceProvider> OnServiceBuilt { get; set; }

    internal override bool TryStart()
    {
        Collection = new ServiceCollection();
        LoaderVersion = new Version();
        Loaders = NextPreLoaderExtension.GetPreLoaders(
                                                       typeof(DesktopLoader).Assembly, 
                                                       typeof(BasePreLoader).Assembly, 
                                                       typeof(Il2CppInteropManager).Assembly
                                                       ).ToList();


        PlatformUtils.SetDesktopPlatformVersion();

        Collection.AddSingleton<IPreLoaderManager, DesktopPreLoadManager>();
        Collection.AddSingleton(Loaders);
        Collection.AddLogging(n => n.AddProvider(new BepInExLoggerProvider()));

        MainServices = Collection.BuildServiceProvider();
        foreach (var start in MainServices.GetServices<IOnLoadStart>())
            start.OnLoadStart();
        
        
        OnServiceBuilt(MainServices);
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
