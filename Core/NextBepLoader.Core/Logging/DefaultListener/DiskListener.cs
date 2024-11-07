using System.IO;
using System.Text;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.Logging.DefaultListener;

public class DiskListener(string path, LogLevel logLevel = LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info) : ILogListener
{
    public LogLevel LogLevelFilter => logLevel;

    public readonly TextWriter? Writer = CreateWriter(path);

    private static TextWriter? CreateWriter(string path)
    {
        var stream = File.OpenWrite(path);
        var writer = new StreamWriter(stream, Utility.UTF8NoBom)
        {
            AutoFlush = true
        };

        return writer;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        Writer?.WriteLine(eventArgs.ToString());
    }
    
    public void Dispose()
    {
        try
        {
            Writer?.Dispose();
            
            if (Logger.Listeners.Contains(this))
                Logger.Listeners.Remove(this);
        }
        catch
        {
            // ignored
        }
    }

    ~DiskListener()
    {
        Dispose();
    }
}
