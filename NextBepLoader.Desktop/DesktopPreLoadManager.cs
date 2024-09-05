using NextBepLoader.Core;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Deskstop;

public class DesktopPreLoadManager(IServiceProvider provider, List<BasePreLoader> loaders) : IPreLoaderManager, IOnLoadStart
{
    public void OnLoadStart()
    {

    }
}
