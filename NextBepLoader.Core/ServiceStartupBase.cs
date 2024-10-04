using System;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core;

public abstract class ServiceStartupBase : IStartup
{
    public abstract void ConfigureServices(IServiceCollection services);

    public virtual T? GetService<T>() where T : class => GetService(typeof(T)) as T;

    public virtual object? GetService(Type type) => null;
}
