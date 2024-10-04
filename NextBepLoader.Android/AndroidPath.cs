using NextBepLoader.Core;

namespace NextBepLoader.Android;

public class AndroidPath : LoaderPathBase
{
    public override string? GameRootPath { get; set; }

    public override void InitPaths(bool autoCheckCreate = false)
    {
        
        base.InitPaths(autoCheckCreate);
    }
}
