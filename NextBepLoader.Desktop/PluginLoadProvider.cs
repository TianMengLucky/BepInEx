using System.Runtime.CompilerServices;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Deskstop;

public class PluginLoadProvider : LoadProviderBase<BasePlugin>
{
    public List<BasePlugin> AllPlugins { get; set; } = [];
    private static readonly string FullName = typeof(BasePlugin).FullName ?? "";
    private ServiceFastInfo? pluginServiceInfo;

    public override void Init(IProviderManager manager)
    {
        pluginServiceInfo = NextServiceManager.Instance.GetServiceInfo("PluginService");
    }

    public override void Run()
    {
        if (pluginServiceInfo == null) return;
        base.Run();
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
        
        return type.FullName.Equals(FullName);
    }

    public override void OnGameActive()
    {
        foreach (var plugin in AllPlugins)
        {
            try
            {
                plugin.Load();
            }
            catch (Exception e)
            {
                Logger.LogError($"{plugin.GetType().FullName} LoadError:\n" + e.Message);
            }
        }
    }

    public record FastPluginMetaInfo
    {
        
    }
}
