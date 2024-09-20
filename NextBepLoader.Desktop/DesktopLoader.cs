using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.IL2CPP.NextPreLoaders;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.NextPreLoaders;
using NextBepLoader.Core.PreLoader.RuntimeFixes;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase<DesktopLoader>
{
    private List<BasePreLoader> loaders = [];
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();
    public override LoaderPlatformType LoaderType => LoaderPlatformType.Desktop;
    internal IServiceCollection Collection { get; set; } = new ServiceCollection();
    public new Action<IServiceProvider> OnServiceBuilt { get; set; }

    public override void Start()
    {
        LoaderVersion = new Version();
        loaders = [
            new ResolvePreLoad(), 
            new IL2CPPHook(), 
            new IL2CPPPreLoader()
        ];
        PlatformUtils.SetDesktopPlatformVersion();
        MainServices = Collection
                       .AddSingleton<IPreLoaderManager, DesktopPreLoadManager>(n => new DesktopPreLoadManager(n, loaders))
                       .AddSingleton<INextServiceManager, NextServiceManager>()
                       .AddNextLogger()
                       .BuildServiceProvider();
        
        
        OnServiceBuilt(MainServices);
        
        
        HarmonyBackendFix.Initialize();
        UnityInfo.InitializeFormPaths();
        RedirectStdErrFix.Apply();
    }
}
