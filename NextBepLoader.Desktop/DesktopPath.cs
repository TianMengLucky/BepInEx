using NextBepLoader.Core;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public class DesktopPath : LoaderPathBase
{
    public override string GameRootPath { get; set; }

    public override void InitPaths()
    {
        var bepinPath =
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(EnvVars.DOORSTOP_INVOKE_DLL_PATH)));
        Paths.SetExecutablePath(EnvVars.DOORSTOP_PROCESS_PATH, bepinPath, EnvVars.DOORSTOP_MANAGED_FOLDER_DIR, false,
                                EnvVars.DOORSTOP_DLL_SEARCH_DIRS);
        base.InitPaths();
    }
}
