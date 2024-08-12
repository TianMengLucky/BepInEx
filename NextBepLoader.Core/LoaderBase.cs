using System;

namespace NextBepLoader.Core;

public abstract class LoaderBase
{
    public abstract LoaderPathBase Paths { get; set; }
    public Version LoaderVersion { get; set; }
    public LoaderBase Current { get; set; }
    
    public IServiceProvider MainServices { get; set; }

    public LoaderBase()
    {
        Current = this;
    }

    internal abstract bool TryStart();
}
