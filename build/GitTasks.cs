using Cake.Common;
using Cake.Core;

namespace Build;

static class GitTasks
{
    public static string Git(this ICakeContext ctx, string args, string separator = "")
    {
        using var process = ctx.StartAndReturnProcess("git", new() { Arguments = args, RedirectStandardOutput = true });
        process.WaitForExit();
        return string.Join(separator, process.GetStandardOutput());
    }
}