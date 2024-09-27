using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core;

public abstract class ServiceStartupBase : IStartup
{
    public abstract void ConfigureServices(IServiceCollection services);
}
