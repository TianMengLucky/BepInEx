using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.Logging.DefaultSource;

namespace NextBepLoader.Core.Contract;

public abstract class BasePlugin : INextPlugin
{
    public virtual PluginMetadata? Metadata { get; set; } = null;
    
    protected BasePlugin()
    {
    }

    public ManualLogSource Log { get; }

    public ConfigFile Config { get; }

    internal void SetMetadata(PluginMetadata metadata)
    {
        Metadata = metadata;
    }

    public abstract void Load();

    public virtual bool Unload() => false;

    public virtual void RegisterApi() { }
}
