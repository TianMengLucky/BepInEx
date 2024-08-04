using System.IO;
using System.Runtime.InteropServices;
using BepInEx.Core;
using BepInEx.Core.Logging;
using MonoMod.Utils;

namespace BepInEx.Unity.IL2CPP.RuntimeFixes;

internal static partial class RedirectStdErrFix
{
    private const int STD_ERROR_HANDLE = -12;
    private const int INVALID_HANDLE_VALUE = -1;
    private const int FILE_SHARE_READ = 1;
    private const int GENERIC_WRITE = 0x40000000;
    private const int CREATE_ALWAYS = 2;
    private const int FILE_ATTRIBUTE_NORMAL = 0x00000080;

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileA", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateFile(string fileName,
                                          uint desiredAccess,
                                          int shareMode,
                                          nint securityAttributes,
                                          int creationDisposition,
                                          int flagsAndAttributes,
                                          nint templateFile);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetStdHandle(int nStdHandle, nint hConsoleOutput);


    public static void Apply()
    {
        if (!PlatformDetection.OS.Is(OSKind.Windows)) return;
        var errorFile = CreateFile(Path.Combine(Paths.BepInExRootPath, "ErrorLog.log"), GENERIC_WRITE,
                                   FILE_SHARE_READ,
                                   0, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
        if (errorFile == INVALID_HANDLE_VALUE)
        {
            Logger.Log(LogLevel.Warning, "Failed to open error log file; skipping error redirection");
            return;
        }

        if (!SetStdHandle(STD_ERROR_HANDLE, errorFile))
            Logger.Log(LogLevel.Warning, "Failed to redirect stderr; skipping error redirection");
        // On unix, we can generally redirect stderr to a file "normally" via piping
    }
}
