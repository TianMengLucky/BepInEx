using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Deskstop;

public class DesktopBepEnv : INextBepEnv, IOnLoadStart
{
    private Action<DesktopBepEnv> OnExited = env => {};
    private Dictionary<string, string> SystemEnvs = new();
    private Dictionary<Type, object> Actions = new();


    public INextBepEnv RegisterSystemEnv(string variable, string value)
    {
        SystemEnvs.Add(variable, value);
        Environment.SetEnvironmentVariable(variable, value);
        return this;
    }

    public Process CurrentProcess { get; set; }

    public INextBepEnv RegisterEventArgs<T>(T arg) where T : EventArgs => throw new NotImplementedException();

    public T? GetEventArgs<T>() where T : EventArgs => throw new NotImplementedException();
    public T GetOrCreateEventArgs<T>() where T : EventArgs, new() => throw new NotImplementedException();

    public INextBepEnv UpdateEventArgs<T>(T arg) where T : EventArgs => throw new NotImplementedException();


    public void OnLoadStart()
    {
        CurrentProcess = Process.GetCurrentProcess();

        CurrentProcess.Exited += OnExit;
    }

    private  void OnExit(object? sender, EventArgs e)
    {
        foreach (var (variable, value) in SystemEnvs)
            Environment.SetEnvironmentVariable(variable, null);
        SystemEnvs.Clear();
        
        OnExited?.Invoke(this);
    }
}
