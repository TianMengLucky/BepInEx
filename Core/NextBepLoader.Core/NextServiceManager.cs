using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core;

public class NextServiceManager : INextServiceManager
{
    public List<IServiceProvider> Providers = [];
    public static NextServiceManager Instance => NextServiceManagerExtension.CreateOrGet();

    public List<ServiceFastInfo> ServiceInfos = [];

    public IServiceProvider MainProvider
    {
        get => MainFastInfo.Provider ?? MainFastInfo.Collection.BuildOrCreateProvider();
        set => MainFastInfo.Provider = value;
    }

    public ServiceFastInfo MainFastInfo
    {
        get;
        set;
    }

    public ServiceFastInfo? GetServiceInfo(string id)
    {
        return ServiceInfos.FirstOrDefault(n => n.Collection.ServiceId == id);
    }


    public NextServiceManager Register(NextServiceCollection collection, IServiceProvider? provider = null)
    {
        if (ServiceInfos.Exists(n => n.Id == collection.ServiceId)) return this;
        var info = new ServiceFastInfo
        {
            Collection = collection,
            Provider = provider,
            Types = collection.Where(x => x.Lifetime == ServiceLifetime.Singleton).Select(x => x.ServiceType).ToList()
        };
        ServiceInfos.Add(info);
        if (collection.ServiceId == "Main")
            MainFastInfo = info;
        return this;
    }

    public NextServiceCollection CreateMainCollection()
    {
        return CreateService("Main");
    }
    
    public NextServiceCollection CreateService(string id)
    {
        return new NextServiceCollection
        {
            ServiceId = id
        }.AddNextServiceManager();
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
    
    public static NextServiceCollection AddNextServiceManager(this NextServiceCollection services)
    {
        var manager = CreateOrGet();
        manager.Register(services);
        services.AddSingleton<INextServiceManager>(manager);
        return services;
    }    
}

public class NextServiceCollection : ServiceCollection
{
    public string ServiceId { get; set; }

    private NextServiceProvider? Provider { get; set; }
    public ServiceFastInfo? FastInfo { get; set; }

    public static readonly string[] NoCopyNames = 
    [
        "IOptions",
        "IOptionsMonitor",
        "IOptionsMonitorCache",
        "ILogger"
    ];
    
    public NextServiceCollection Copy(IServiceCollection collection, IServiceProvider provider)
    {
        foreach (var service in collection.Where(n => n.Lifetime == ServiceLifetime.Singleton && !n.IsKeyedService))
        {
            try
            {
                if (Contains(service)) continue;
                if (NoCopyNames.Contains(service.ServiceType.Name)) continue;
                var get = provider.GetService(service.ServiceType);
                if (get == null) continue;
                this.AddSingleton(service.ServiceType, get);
                Logger.LogInfo($"Copy Get Type:{service.ServiceType.Name} Imp:{service.ImplementationType?.Name ?? "null"}");
            }
            catch
            {
                Logger.LogError($"Copy Get Type {service.ServiceType.Name}");
            }
        }

        return this;
    }

    public NextServiceProvider BuildOrCreateProvider()
    {
        if (FastInfo?.Provider != null)
            return (NextServiceProvider)FastInfo.Provider;
        
        if (Provider != null)
            return Provider;
        
        Provider = new NextServiceProvider(this.BuildServiceProvider(), this);
        NextServiceManager.Instance.Register(this, Provider);
        if (FastInfo != null)
            FastInfo.Provider = Provider;
        return Provider;
    }
}

public class NextServiceProvider(IServiceProvider baseProvider, NextServiceCollection collection) : IServiceProvider
{
    public IServiceProvider _Provider { get; set; } = baseProvider;
    public NextServiceCollection Collection { get; set; } = collection;

    public object? GetService(Type serviceType)
    {
        return _Provider.GetService(serviceType);
    }
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
    public NextServiceCollection Collection;

    public string Id => Collection.ServiceId;

    public NextServiceCollection CreateCollection()
    {
        return Collection = [];
    }
    
    public void Copy(ServiceFastInfo info)
    {
        var collection = new NextServiceCollection();

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
