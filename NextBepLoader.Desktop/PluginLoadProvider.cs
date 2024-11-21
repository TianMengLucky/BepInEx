using System.Runtime.CompilerServices;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Deskstop;

public class PluginLoadProvider(ILogger<PluginLoadProvider> logger, DotNetLoader loader, PluginInfoManager pluginInfoManager)
    : LoadProviderBase<BasePlugin>(loader)
{
    private static readonly string FullName = typeof(BasePlugin).FullName ?? "";
    private ServiceFastInfo? pluginServiceInfo;
    public bool GameActivated { get; private set; } = false;

    public override void Init(IProviderManager manager)
    {
        pluginServiceInfo = NextServiceManager.Instance.GetServiceInfo("PluginService");
    }
    
    public PluginLoadProvider LoadSingleAssemblyPlugin(string path)
    {
        return this;
    }

    protected override BasePlugin? Selector(FastTypeFinder.FindInfo info)
    {
        if (info.AssemblyType == null)
            return null;
        try
        {
            RuntimeHelpers.RunModuleConstructor(info.AssemblyType.Module.ModuleHandle);
        }
        catch
        {
            // ignored
        }
        
        return
            ActivatorUtilities.CreateInstance(
                                              pluginServiceInfo?.Collection.BuildOrCreateProvider() 
                                            ?? 
                                              NextServiceManager.Instance.MainProvider, 
                                              info.AssemblyType
                                              ) as BasePlugin;
    }

    protected override bool IsTarget(TypeDefinition type)
    {
        /*MetadataHelper.GetCustomAttributes<>()*/
        var baseType = type.BaseType;
        if (baseType == null)
        {
            logger.LogInformation("{type} Base IsNull", type.FullName);
            return false;
        }
        
        var isTarget = baseType.FullName.Equals(FullName);
        logger.LogInformation($"is Target: {baseType.FullName} {FullName} {isTarget}");
        return isTarget;
    }

    public override void OnGameActive()
    {
        foreach (var plugin in AllSelect)
        {
            try
            {
                plugin.Load();
                logger.LogInformation($"Load {plugin.GetType().FullName}");
            }
            catch (Exception e)
            {
                logger.LogError($"{plugin.GetType().FullName} LoadError:\n{e}");
            }
        }

        GameActivated = true;
    }
}

public class PluginLoadContext : IContent
{
    public string Name { get; set; }
    
    public string Puid { get; set; }
    public Version Version { get; set; }
    
    public bool Active { get; set; }
    
    public bool HasLoad { get; set; }
}

public class PluginInfoManager : IInfoManager
{

    private List<PluginLoadContext> allLoadContexts = [];
    
    
}
