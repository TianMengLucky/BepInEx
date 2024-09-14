using NextBepLoader.Core;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Deskstop;

public class DesktopPreLoadManager(IServiceProvider provider, List<BasePreLoader> loaders) : IPreLoaderManager, IOnLoadStart
{
    public List<BasePreLoader> PreLoaders = loaders;
    public void OnLoadStart()
    {
    }
}
