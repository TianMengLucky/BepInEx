using System;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class IL2CPPInteropStarter : BasePreLoader
{
    public override Type[] WaitLoadLoader => [typeof(Cpp2ILStarter)];
}
