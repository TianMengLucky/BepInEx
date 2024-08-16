using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace NextBepLoader.Core.PreLoader.NextPreLoaders;

public static class NextPreLoaderExtension
{
    internal static readonly List<BasePreLoader> Cache = [];

    public static IEnumerable<BasePreLoader> GetPreLoaders(params Assembly[] allAssembly) => allAssembly.SelectMany(assembly => assembly.GetPreLoadersFormAssembly(type => type.Namespace != null && type.Namespace.EndsWith("NextPreLoaders")));

    public static List<BasePreLoader> GetPreLoadersFormDef() =>
        typeof(NextPreLoaderExtension).Assembly.GetPreLoadersFormAssembly(type => type.Namespace ==
                                                                              typeof(NextPreLoaderExtension).Namespace);

    public static List<BasePreLoader> GetPreLoadersFormAssembly(this Assembly assembly,
                                                                Predicate<Type>? predicate = null)
    {
        var findList = new List<BasePreLoader>();

        foreach (var findType in assembly.GetTypes().Where(IsTarget))
        {
            if (TryGetCachePreLoader(findType, out var preLoader))
            {
                findList.Add(preLoader);
                continue;
            }

            var instance = (BasePreLoader)Activator.CreateInstance(findType);
            Cache.Add(instance);
            findList.Add(instance);
        }

        return findList;

        bool IsTarget(Type type)
        {
            if (predicate != null) return type.BaseType == typeof(BasePreLoader) && predicate(type);

            return type.BaseType == typeof(BasePreLoader);
        }
    }

    public static T? GetCachePreLoader<T>() where T : BasePreLoader => Cache.FirstOrDefault(n => n is T) as T;

    public static BasePreLoader? GetCachePreLoader(Type type) => Cache.FirstOrDefault(n => n.GetType() == type);

    public static bool TryGetCachePreLoader(Type type, [MaybeNullWhen(false)] out BasePreLoader preLoader)
    {
        var loader = GetCachePreLoader(type);
        if (loader == null)
        {
            preLoader = null;
            return false;
        }

        preLoader = loader;
        return true;
    }
}
