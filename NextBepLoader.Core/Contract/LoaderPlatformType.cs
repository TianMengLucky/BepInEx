using System;

namespace NextBepLoader.Core.Contract;

[Flags]
public enum LoaderPlatformType
{
    Core = 0,
    Desktop = Core | 1,
    Android = Core | 2
}
