using System;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Core.PreLoader;

public class LoadProviderBase<TPlugin> : IProvider
{
    public static LoadProviderBase<TPlugin> Instance;
    public DotNetLoader? _DotNetLoader;
    protected LoadProviderBase()
    {
        Instance = this;
    }

    public virtual void Init(IProviderManager manager)
    {
        _DotNetLoader = manager.MainServiceProvider.GetService<DotNetLoader>();
    }
    public virtual void Run(){}
    public virtual void OnGameActive(){}
}
