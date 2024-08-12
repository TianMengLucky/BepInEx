using System.IO;
using System.Text;
using HarmonyLib;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader.RuntimeFixes;

public static class ConsoleSetOutFix
{
    private static LoggedTextWriter loggedTextWriter;
    internal static ManualLogSource ConsoleLogSource = Logger.CreateLogSource("Console");

    public static void Apply()
    {
        loggedTextWriter = new LoggedTextWriter { Parent = System.Console.Out };
        System.Console.SetOut(loggedTextWriter);
        Harmony.CreateAndPatchAll(typeof(ConsoleSetOutFix));
    }

    [HarmonyPatch(typeof(System.Console), nameof(System.Console.SetOut))]
    [HarmonyPrefix]
    private static bool OnSetOut(TextWriter newOut)
    {
        loggedTextWriter.Parent = newOut;
        return false;
    }
}

internal class LoggedTextWriter : TextWriter
{
    public override Encoding Encoding { get; } = Encoding.UTF8;

    public TextWriter Parent { get; set; }

    public override void Flush() => Parent.Flush();

    public override void Write(string value)
    {
        ConsoleSetOutFix.ConsoleLogSource.Log(LogLevel.Info, value);
        Parent.Write(value);
    }

    public override void WriteLine(string value)
    {
        ConsoleSetOutFix.ConsoleLogSource.Log(LogLevel.Info, value);
        Parent.WriteLine(value);
    }
}
