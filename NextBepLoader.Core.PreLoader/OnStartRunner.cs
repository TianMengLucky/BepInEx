using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core.PreLoader;

internal class OnStartRunner
{
    internal OnStartRunner(IServiceProvider provider) => Run(provider);

    private static void Run(IServiceProvider provider)
    {
        var allStart = provider.GetServices<IOnLoadStart>().ToList();
        allStart.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        foreach (var start in allStart)
            start.OnLoadStart();
    }
}
