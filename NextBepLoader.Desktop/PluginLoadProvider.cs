using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP.NextPreLoaders;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Deskstop;

public class PluginLoadProvider : LoadProviderBase<BasePlugin>
{
    public List<BasePlugin> AllPlugins { get; set; } = [];

    public override void Run()
    {
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
}
