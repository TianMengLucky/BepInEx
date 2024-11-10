using System.Runtime.CompilerServices;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Deskstop;

public class PluginLoadProvider(ILogger<PluginLoadProvider> logger, DotNetLoader loader)
    : LoadProviderBase<BasePlugin>(loader)
{
    private static readonly string FullName = typeof(BasePlugin).FullName ?? "";
    private ServiceFastInfo? pluginServiceInfo;

    public override void Init(IProviderManager manager)
    {
        pluginServiceInfo = NextServiceManager.Instance.GetServiceInfo("PluginService");
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
        logger.LogInformation($"is Target: {type.FullName} {type.FullName.Equals(FullName)}");
        return type.FullName.Equals(FullName);
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
                logger.LogError($"{plugin.GetType().FullName} LoadError:\n{e.ToString()}");
            }
        }
    }
    
}
