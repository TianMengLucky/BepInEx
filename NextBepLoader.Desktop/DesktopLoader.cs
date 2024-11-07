using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP;
using NextBepLoader.Core.IL2CPP.NextPreLoaders;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.DefaultListener;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;
using NextBepLoader.Core.PreLoader.NextPreLoaders;
using NextBepLoader.Core.Utils;
using NextBepLoader.Deskstop.Console;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase<DesktopLoader>
{
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();
    public override LoaderPlatformType LoaderType => LoaderPlatformType.Desktop;
    internal NextServiceCollection Collection { get; set; }
    private static readonly string CoreAssemblyName = typeof(LoaderInstance).Assembly.GetName().Name!;

    public override IConsoleManager ConsoleManager => DesktopConsoleManager.Instance;

    private static readonly string[] FullNames = 
    [
        typeof(BasePlugin).FullName!,
        typeof(BasePreLoader).FullName!,
        typeof(IProvider).FullName!,
        typeof(IStartup).FullName!
    ];
    
    
    private static readonly DotNetLoader DotNetLoader = new()
    {
        AssemblyFilter = AssemblyFilter
    };

    private static bool AssemblyFilter(AssemblyDefinition assembly)
    {
        if (assembly.ManifestModule == null)
            return false;

        var typeReferences = assembly.ManifestModule.GetImportedTypeReferences().ToList();
        var references = assembly.ManifestModule.AssemblyReferences.ToList();
        if (references.All(n => !n.Name!.Equals(CoreAssemblyName)))
            return false;

        return !typeReferences.All(n => FullNames.All(name => !n.FullName.Equals(name)));
    }
    

    public ILogListener? DiskLogListener { get; private set; }
    public override void Start()
    {
        PlatformUtils.SetDesktopPlatformVersion();
        RedirectStdErrFix.Apply();
        
        DiskLogListener = new DiskListener("./LatestLog.log").Register();
        ConsoleManager.Init(new ConsoleConfig());
        ConsoleManager.CreateConsole();
        
        Logger.LogInfo("Test");
        
        LoaderVersion = new Version(1, 0, 0);
        Paths.InitPaths(true);
        
        DotNetLoader.AddAssembliesFormDirector(Paths.PluginPath);
        Type[] loaders = [
            typeof(ResolvePreLoad),
            typeof(Cpp2ILStarter),
            typeof(HashComputer),
            typeof(IL2CPPHooker), 
            typeof(UnityBasePreDownloader),
            typeof(IL2CPPInteropStarter),
            typeof(IL2CPPPreLoader)
        ];

        Collection = BuildService(loaders, this);
        MainServices = Collection.BuildOrCreateProvider();
    }

    internal static NextServiceCollection BuildService(Type[] loaders, DesktopLoader loader)
    {
        var collection = NextServiceManager.Instance.CreateMainCollection();
        collection
            .AddSingleton(DesktopConsoleManager.Instance)
            .AddSingleton(DotNetLoader)
            .AddSingleton(loader)
            .AddSingleton<TaskFactory>()
            .AddSingleton<IProviderManager, DesktopProviderManager>()
            .AddSingleton<UnityInfo>()
            .AddOnStart<INextBepEnv, DesktopBepEnv>()
            .AddOnStart<IPreLoaderManager, DesktopPreLoadManager>(loaders, new PreLoadEventArg())
            .AddStartRunner()
            .AddNextLogger()
            .AddTraceLogSource();
        return collection;
    }
}
