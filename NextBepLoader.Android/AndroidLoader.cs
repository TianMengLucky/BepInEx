using NextBepLoader.Core;
using NextBepLoader.Core.Contract;

namespace NextBepLoader.Android;

public sealed class AndroidLoader : LoaderBase<AndroidLoader>
{
    public override LoaderPathBase Paths { get; set; } = new AndroidPath();
    public override LoaderPlatformType LoaderType => LoaderPlatformType.Android;

    public override void Start() 
    {
        
    }
}
