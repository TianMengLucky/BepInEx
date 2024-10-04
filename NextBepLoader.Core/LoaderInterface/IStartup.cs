using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.LoaderInterface;

public interface IStartup
{
    public void ConfigureServices(IServiceCollection services);
}
