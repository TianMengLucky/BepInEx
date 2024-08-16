using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.PreLoader;

public abstract class BasePreLoader
{
    public virtual PreLoadPriority Priority { get; set; } = PreLoadPriority.Default;
    
    public virtual void Start() { }

    public virtual void PreLoad(PreLoadEventArg arg) { }

    public virtual void LoaderServiceBuild(IServiceCollection collection, LoaderBase loader) { }

    public virtual void PluginServiceBuild(IServiceCollection collection, LoaderBase loader) { }
}
