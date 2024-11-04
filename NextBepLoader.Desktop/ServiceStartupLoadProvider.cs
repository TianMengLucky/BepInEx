using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Deskstop;

public class ServiceStartupLoadProvider : LoadProviderBase<IStartup>
{
    private NextServiceCollection? Service { get; set; }
    private DotNetLoader? Loader { get; set; }
    public List<IStartup> Startups { get; private set; } = [];

    public override void Init(IProviderManager manager)
    {
        Service = NextServiceManager.Instance.CreateService("PluginService");
        Loader = manager.MainServiceProvider.GetService<DotNetLoader>();
        if (Service == null)
            Logger.LogError("Service not found");
        
        if (Loader == null)
            Logger.LogError("Loader not found");

        var collection = NextServiceManager.Instance.MainFastInfo.Collection;
        Service?.Copy(collection, collection.BuildOrCreateProvider());
    }

    public override void Run()
    {
        if (Service == null || Loader == null) return;
        Startups = new FastTypeFinder()
                   .FindFormTypeLoader(Loader, IsStartup)
                   .SelectTo(Selector);
        
        foreach (var startup in Startups)
        {
            startup.ConfigureServices(Service);
        }
    }

    private static readonly string BaseFullName = typeof(ServiceStartupBase).FullName ?? "";
    private static readonly string InterfaceFullName = typeof(IStartup).FullName ?? "";
    
    private static IStartup? Selector(FastTypeFinder.FindInfo info)
    {
        if (info.AssemblyType == null)
            return null;
        return Activator.CreateInstance(info.AssemblyType) as IStartup;
    }
    private static bool IsStartup(TypeDefinition type)
    {
        return 
            type.BaseType?.FullName.Equals(BaseFullName) 
            ?? 
            type.Interfaces.Any(n => n.Interface?.FullName.Equals(InterfaceFullName) ?? false);
    }
}
