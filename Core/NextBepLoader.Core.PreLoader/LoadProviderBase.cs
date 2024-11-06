using System.Collections.Generic;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Core.PreLoader;

public class LoadProviderBase<TPlugin> : IProvider
{
    public static LoadProviderBase<TPlugin> Instance;
    public DotNetLoader? _DotNetLoader;
    public FastTypeFinder Finder;
    public List<TPlugin> AllSelect = [];
    protected LoadProviderBase()
    {
        Instance = this;
    }

    public virtual void Init(IProviderManager manager)
    {
        _DotNetLoader = manager.MainServiceProvider.GetService<DotNetLoader>();
        Finder = new FastTypeFinder();
    }

    public virtual void Run()
    {
        if (_DotNetLoader == null) return;
        AllSelect = Finder
                    .FindFormTypeLoader(_DotNetLoader, IsTarget)
                    .SelectTo(Selector);
    }
    public virtual void OnGameActive(){}

    protected virtual TPlugin? Selector(FastTypeFinder.FindInfo info)
    {
        return default;
    }

    protected virtual bool IsTarget(TypeDefinition type)
    {
        return true;
    }
}
