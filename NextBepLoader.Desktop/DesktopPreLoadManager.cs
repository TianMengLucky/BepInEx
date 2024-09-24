using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.NextPreLoaders;

namespace NextBepLoader.Deskstop;

public class DesktopPreLoadManager(IServiceProvider provider, List<Type> loaderTypes, TaskFactory factory, PreLoadEventArg loadEventArg) : IPreLoaderManager, IOnLoadStart
{
    public List<BasePreLoader> PreLoaders { get; set; } = [];
    public void OnLoadStart()
    {
        foreach (var loader in loaderTypes.Select(type => ActivatorUtilities.CreateInstance(provider, type)))
        {
            if (loader is BasePreLoader basePreLoader)
            {
                PreLoaders.Add(basePreLoader);
            }
        }

        PreLoaders.Sort();
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
