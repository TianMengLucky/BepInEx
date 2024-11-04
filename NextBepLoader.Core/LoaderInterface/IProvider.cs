namespace NextBepLoader.Core.LoaderInterface;

public interface IProvider
{
    public void Init(IProviderManager manager);
    public void Run();

    public void OnGameActive();
}
