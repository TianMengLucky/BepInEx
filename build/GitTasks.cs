using Cake.Common;
using Cake.Core;
using Cake.Core.IO;

namespace Build;

internal static class GitTasks
{
    public static string Git(this ICakeContext ctx, string args, string separator = "")
    {
        using var process =
            ctx.StartAndReturnProcess("git", new ProcessSettings { Arguments = args, RedirectStandardOutput = true });
        process.WaitForExit();
        return string.Join(separator, process.GetStandardOutput());
    }
}
