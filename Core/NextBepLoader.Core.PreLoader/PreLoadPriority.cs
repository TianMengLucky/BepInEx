namespace NextBepLoader.Core.PreLoader;

public enum PreLoadPriority : int
{
    VeryLast = 0,
    Last = 10,
    Default = 20,
    Lowest = 40,
    VeryLowest = 50,
}
