using System;
using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.LoaderInterface;

public interface IProviderManager
{
    public T? GetProvider<T>() where T : IProvider;
    
    public IServiceProvider MainServiceProvider { get; }

    public void OnGameActive();
}
