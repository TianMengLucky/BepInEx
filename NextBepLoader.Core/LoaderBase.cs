using System;
using NextBepLoader.Core.Contract;

namespace NextBepLoader.Core;

public abstract class LoaderBase
{
    public LoaderBase()
    {
        Current = this;
    }

    public static LoaderBase Current { get; internal set; }

    public abstract LoaderPathBase Paths { get; set; }
    public abstract LoaderPlatformType LoaderType { get; }
    public Version LoaderVersion { get; set; }

    public IServiceProvider MainServices { get; set; }

    public Action<IServiceProvider>? OnServiceBuilt { get; set; }

    internal abstract bool TryStart();
}
