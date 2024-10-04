using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using MonoMod.Utils;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core.Utils;

public static class CoreUtils
{
    public static readonly bool IsMono = PlatformDetection.Runtime == RuntimeKind.Mono;

    public static readonly bool IsCore = PlatformDetection.Runtime == RuntimeKind.CoreCLR;

    public static string PlatformPostFix => PlatformPostFixGet();
    public static string PlatformGameAssemblyName => PlatformGameAssemblyNameGet();

    public static T? GetExportAsDelegate<T>(this IntPtr s, string name) where T : class =>
        s.GetExport(name).AsDelegate<T>();

    public static T? AsDelegate<T>(this IntPtr s) where T : class =>
        Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;
    
    private static string PlatformGameAssemblyNameGet()
    {
        if (PlatformDetection.OS.Is(OSKind.Android))
            return "libil2cpp";
        
        return "GameAssembly";
    }

    private static string PlatformPostFixGet()
    {
        if (PlatformDetection.OS.Is(OSKind.Android))
            return "so";
        
        if (PlatformDetection.OS.Is(OSKind.OSX))
            return "dylib";

        if (PlatformDetection.OS.Is(OSKind.Posix))
            return "sp";

        return "dll";
    }

    public static TimeSpan StartStopwatch(Action action)
    {
        var watch = new Stopwatch();
        watch.Start();
        action.Invoke();
        watch.Stop();
        return watch.Elapsed;
    }

    public static IServiceCollection AddOnStart<T>(this IServiceCollection collection) where T : class, IOnLoadStart =>
        collection.AddSingleton<IOnLoadStart, T>();

    public static IServiceCollection AddOnStart<TInterface, TClass>(this IServiceCollection collection)where TClass : class, TInterface, IOnLoadStart where TInterface : class
    {
        return collection.AddSingleton<TInterface, TClass>()
                  .AddSingleton<IOnLoadStart>(n => n.GetRequiredService<TClass>());
    }

    public static IServiceCollection AddOnStart<TInterface, TClass>
    (
        this IServiceCollection collection, 
        params object[] parameters
        )
        where TClass : class, TInterface, IOnLoadStart 
        where TInterface : class
        => collection
           .AddSingleton<TInterface, TClass>(provider => ActivatorUtilities.CreateInstance<TClass>(provider, parameters))
           .AddOnStartFormGet<TClass>();
    
    public static IServiceCollection AddOnStartFormGet<T>(this IServiceCollection collection) where T : class, IOnLoadStart =>
        collection.AddSingleton<IOnLoadStart>(n => n.GetRequiredService<T>());

    public static void DeleteAllFiles(string dir)
    {
        if (!Directory.Exists(dir))
            return;
        
        Directory
            .GetFiles(dir)
            .Do(File.Delete);
    }
    
    public static void HashString(this ICryptoTransform hash, string str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        hash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
    }

    public static void HashFile(this ICryptoTransform hash, string file)
    {
        const int defaultCopyBufferSize = 81920;
        using var fs = File.OpenRead(file);
        var buffer = new byte[defaultCopyBufferSize];
        int read;
        while ((read = fs.Read(buffer)) > 0)
            hash.TransformBlock(buffer, 0, read, buffer, 0);
    }

    public static T GetOrCreate<T>(this List<T> list, Func<T, bool> predicate, Func<T> create) where T : class
    {
        var item = list.FirstOrDefault(predicate);
        if (item is not null) return item;
        item = create();
        list.Add(item);

        return item;        
    }

    public static bool GetAndSet<T>(this List<T> list, Func<T, bool> predicate, Action<T> set)
    {
        var item = list.FirstOrDefault(predicate);
        if (item is null) return false;
        set(item);
        return true;
    }

    public static bool TryGet<T>(this List<T> list, Func<T, bool> predicate,[MaybeNullWhen(false)] out T item) where T : class
    {
        item = list.FirstOrDefault(predicate);
        return item is not null;
    }
}
