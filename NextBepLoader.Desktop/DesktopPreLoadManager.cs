using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.NextPreLoaders;

namespace NextBepLoader.Deskstop;

public class DesktopPreLoadManager(IServiceProvider provider, Type[] loaderTypes, TaskFactory factory, PreLoadEventArg loadEventArg) : IPreLoaderManager, IOnLoadStart
{
    public int Priority => 0;
    public List<BasePreLoader> PreLoaders { get; set; } = [];

    public T? GetPreLoader<T>() where T : BasePreLoader => PreLoaders.FirstOrDefault(n => n is T) as T;
    
    public void OnLoadStart()
    {
        foreach (var loader in loaderTypes.Select(type => ActivatorUtilities.CreateInstance(provider, type)))
        {
            if (loader is BasePreLoader basePreLoader)
            {
                PreLoaders.Add(basePreLoader);
            }
        }
        
        PreLoaders.SortLoaders();
        factory.StartNew(LoadPreLoad);
    }

    public void LoadPreLoad()
    {
        foreach (var loader in PreLoaders)
            loader.PreLoad(loadEventArg);
        
        foreach (var loader in PreLoaders)
            loader.Start();

        foreach (var loader in PreLoaders)
            loader.Finish();
    }
}
