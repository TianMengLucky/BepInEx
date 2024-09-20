using System;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class Cpp2ILStarter : BasePreLoader
{
    public override Type[] WaitLoadLoader { get; set; } 
}
