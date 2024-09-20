using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.Contract;

public abstract class BasePlugin
{
    public virtual PluginDependency[] Dependencies { get; set; } = [];
    
    public virtual PluginProcess Process { get; set; } = new PluginProcess("");
    public virtual PluginMetadata? Metadata { get; set; } = null;

    
    protected BasePlugin()
    {
    }

    public ManualLogSource Log { get; }

    public ConfigFile Config { get; }

    public abstract void Load();

    public virtual bool Unload() => false;
}
