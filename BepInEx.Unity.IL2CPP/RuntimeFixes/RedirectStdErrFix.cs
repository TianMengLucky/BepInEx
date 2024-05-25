using System.IO;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using MonoMod.Utils;

namespace BepInEx.IL2CPP.RuntimeFixes;

internal static class RedirectStdErrFix
{
    private const int STD_ERROR_HANDLE = -12;
    private const int INVALID_HANDLE_VALUE = -1;
    private const int FILE_SHARE_READ = 1;
    private const int GENERIC_WRITE = 0x40000000;
    private const int CREATE_ALWAYS = 2;
    private const int FILE_ATTRIBUTE_NORMAL = 0x00000080;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CreateFile(string fileName,
                                          uint desiredAccess,
                                          int shareMode,
                                          nint securityAttributes,
                                          int creationDisposition,
                                          int flagsAndAttributes,
                                          nint templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(int nStdHandle, nint hConsoleOutput);


    public static void Apply()
    {
        // On unix, we can generally redirect stderr to a file "normally" via piping
    }
}
