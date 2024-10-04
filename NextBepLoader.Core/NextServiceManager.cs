using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core;

public class NextServiceManager : INextServiceManager
{
    public List<IServiceProvider> Providers = [];
    public static NextServiceManager Instance => LoaderInstance.GetOrCreateManager<NextServiceManager>();

    public List<ServiceFastInfo> ServiceInfos = [];

    public IServiceProvider MainProvider
    {
        get;
        set;
    }


    public NextServiceManager Register(IServiceCollection collection, IServiceProvider? provider = null)
    {
        ServiceInfos.Add(new ServiceFastInfo
        {
            Collection = collection,
            Provider = provider,
            Types = collection.Where(x => x.Lifetime == ServiceLifetime.Singleton).Select(x => x.ServiceType).ToList()
        });
        return this;
    }
    
    public IServiceCollection CreateService(string id)
    {
        return new ServiceCollection();
    }
    

    public T GetService<T>(string id)
    {
        throw new NotImplementedException();
    }

    public T GetServiceFormAll<T>()
    {
        foreach (var value in Providers.Select(serviceProvider => serviceProvider.GetService<T>()).OfType<T>())
        {
            return value;
        }

        return ActivatorUtilities.GetServiceOrCreateInstance<T>(MainProvider);
    }

    public IServiceProvider Build(string id, params Type[] types)
    {
        throw new NotImplementedException();
    }
}

public static class NextServiceManagerExtension
{
    public static NextServiceManager CreateOrGet() => LoaderInstance.GetOrCreateManager<NextServiceManager>();
    
    public static IServiceCollection AddNextServiceManager(this IServiceCollection services)
    {
        var manager = CreateOrGet();
        manager.Register(services);
 
        services.AddSingleton<INextServiceManager>(manager);
        return services;
    }    
}

public class NextServiceCollection : ServiceCollection, IServiceCollection
{
}

public class NextServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType) => throw new NotImplementedException();
}

public class NextServiceDescriptor : ServiceDescriptor
{
    public NextServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime)
    {
    }

    public NextServiceDescriptor(Type serviceType, object? serviceKey, Type implementationType, ServiceLifetime lifetime) : base(serviceType, serviceKey, implementationType, lifetime)
    {
    }

    public NextServiceDescriptor(Type serviceType, object instance) : base(serviceType, instance)
    {
    }

    public NextServiceDescriptor(Type serviceType, object? serviceKey, object instance) : base(serviceType, serviceKey, instance)
    {
    }

    public NextServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime)
    {
    }

    public NextServiceDescriptor(Type serviceType, object? serviceKey, Func<IServiceProvider, object?, object> factory, ServiceLifetime lifetime) : base(serviceType, serviceKey, factory, lifetime)
    {
    }
}

public class ServiceFastInfo
{
    public List<Type> Types = [];
    public IServiceProvider? Provider;
    public IServiceCollection Collection;

    public IServiceCollection CreateCollection()
    {
        return Collection =  new NextServiceCollection();
    }
    
    public void Copy(ServiceFastInfo info)
    {
        var collection = new ServiceCollection();

        foreach (var type in Types)
        {
            var tryGet = Provider?.GetService(type);
            if (tryGet != null)
                collection.AddSingleton(type, tryGet);
            else
                collection.AddSingleton(type);
        }
        
        foreach (var type in info.Types)
        {
            if (Types.Contains(type)) continue;
            var tryGet = Provider?.GetService(type);
            if (tryGet != null)
                collection.AddSingleton(type, tryGet);
            else
                collection.AddSingleton(type);
        }

        Collection = collection; 
        Provider = collection.BuildServiceProvider();
    }
}
