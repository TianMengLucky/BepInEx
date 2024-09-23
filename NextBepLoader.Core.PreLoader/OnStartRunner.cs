using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.PreLoader;

public class OnStartRunner
{
    public OnStartRunner(IServiceProvider provider) => Run(provider);

    public static void Run(IServiceProvider provider)
    {
        foreach (var start  in provider.GetServices<IOnLoadStart>())
            start.OnLoadStart();
    }
}
