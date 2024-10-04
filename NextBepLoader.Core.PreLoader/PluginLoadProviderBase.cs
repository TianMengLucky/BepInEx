using NextBepLoader.Core.Contract;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core.PreLoader;

public class PluginLoadProviderBase<TPlugin> : IProvider where TPlugin : INextPlugin
{
    public static PluginLoadProviderBase<TPlugin> Instance;
    protected PluginLoadProviderBase()
    {
        Instance = this;
    }
}
