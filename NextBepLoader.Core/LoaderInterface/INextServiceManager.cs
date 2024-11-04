using System;
using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.LoaderInterface;

public interface INextServiceManager
{
    public NextServiceManager Register(NextServiceCollection collection, IServiceProvider? provider = null);
}
