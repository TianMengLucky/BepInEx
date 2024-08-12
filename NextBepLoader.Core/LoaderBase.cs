using System;

namespace NextBepLoader.Core;

public abstract class LoaderBase
{
    public static LoaderBase Current { get; internal set; }
    
    public abstract LoaderPathBase Paths { get; set; }
    public Version LoaderVersion { get; set; }
    
    public IServiceProvider MainServices { get; set; }

    public LoaderBase()
    {
        Current = this;
    }

    internal abstract bool TryStart();
}
