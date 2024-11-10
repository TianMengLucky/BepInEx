using System.Diagnostics;
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

    public INextBepEnv RegisterEventArgs<T>(T arg) where T : EventArgs
    {
        Actions.Add(typeof(T), arg);
        return this;
    }

    public T? GetEventArgs<T>() where T : EventArgs
    {
        return Actions.FirstOrDefault(n => n.Key == typeof(T)).Value as T;
    }

    public T GetOrCreateEventArgs<T>() where T : EventArgs, new()
    {
        if (Actions.TryGetValue(typeof(T), out var value))
        {
            return (T)value;
        }

        var t = new T();
        Actions.Add(typeof(T), t);
        return t;
    }

    public INextBepEnv UpdateEventArgs<T>(T arg) where T : EventArgs
    {
        Actions[typeof(T)] = arg;
        return this;
    }


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
        
        OnExited(this);
    }
}
