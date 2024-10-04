using System;
using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.LoaderInterface;

public interface INextServiceManager
{
    public NextServiceManager Register(IServiceCollection collection, IServiceProvider? provider = null);
}
