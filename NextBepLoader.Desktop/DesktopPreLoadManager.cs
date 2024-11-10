using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.NextPreLoaders;

namespace NextBepLoader.Deskstop;

public class DesktopPreLoadManager(ILogger<DesktopPreLoadManager> logger, IServiceProvider provider, DesktopLoader loader) : IPreLoaderManager, IOnLoadStart
{
    public int Priority => 0;
    public List<BasePreLoader> PreLoaders { get; set; } = [];
    private List<Type> LoaderTypes => loader.DefaultPreLoaderTypes;

    public T? GetPreLoader<T>() where T : BasePreLoader => PreLoaders.FirstOrDefault(n => n is T) as T;
    
    public void OnLoadStart()
    {
        foreach (var preLoader in LoaderTypes.Select(type => ActivatorUtilities.CreateInstance(provider, type)))
        {
            if (preLoader is BasePreLoader basePreLoader)
            {
                PreLoaders.Add(basePreLoader);
            }
        }
        
        PreLoaders.SortLoaders();
        LoadPreLoad();
    }

    public void LoadPreLoad()
    {
        foreach (var preLoader in PreLoaders)
        {
            try
            {
                logger.LogInformation("Run PreLoader:{name}", preLoader.GetType().Name);
                preLoader.PreLoad(loader.PreLoadEventArg);
            }
            catch (Exception e)
            {
                logger.LogError(
                                e, 
                                "PreLoader:{name} PreLoad Error\n {exception}", 
                                preLoader.GetType().Name,
                                e.ToString()
                                );
            }
        }

        foreach (var preLoader in PreLoaders)
        {
            try
            {
                preLoader.Start();
            }
            catch (Exception e)
            {
                logger.LogError(
                                e, 
                                "PreLoader:{name} Start Error\n {exception}", 
                                preLoader.GetType().Name,
                                e.ToString()
                               );
            }
        }

        foreach (var preLoader in PreLoaders)
        {
            try
            {
                preLoader.Finish();
            }
            catch (Exception e)
            {
                logger.LogError(
                                e, 
                                "PreLoader:{name} Finish Error\n {exception}", 
                                preLoader.GetType().Name,
                                e.ToString()
                               );
            }
        }
    }
}
