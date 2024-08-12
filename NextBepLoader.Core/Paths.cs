namespace NextBepLoader.Core;

/// <summary>
///     Paths used by BepInEx
/// </summary>
public static class Paths
{
    public static LoaderPathBase? MainInstance
    {
        get;
        internal set;
    }

    /// <summary>
    ///     The path to the Managed folder that contains the main managed assemblies.
    /// </summary>
    public static string ManagedPath => MainInstance?.ManagedPath ?? string.Empty;

    /// <summary>
    ///     The path to the game data folder of the currently running Unity game.
    /// </summary>
    public static string GameDataPath => MainInstance?.GameDataPath ?? string.Empty;

    /// <summary>
    ///     The directory that the core BepInEx DLLs reside in.
    /// </summary>
    public static string BepInExAssemblyDirectory => MainInstance?.BepInExAssemblyDirectory ?? string.Empty;

    /// <summary>
    ///     The path to the core BepInEx DLL.
    /// </summary>
    public static string BepInExAssemblyPath => MainInstance?.BepInExAssemblyPath ?? string.Empty;

    /// <summary>
    ///     The path to the main BepInEx folder.
    /// </summary>
    public static string BepInExRootPath => MainInstance?.BepInExRootPath ?? string.Empty;

    /// <summary>
    ///     The path of the currently executing program BepInEx is encapsulated in.
    /// </summary>
    public static string ExecutablePath => MainInstance?.ExecutablePath ?? string.Empty;

    /// <summary>
    ///     The directory that the currently executing process resides in.
    ///     <para>On OSX however, this is the parent directory of the game.app folder.</para>
    /// </summary>
    public static string GameRootPath => MainInstance?.GameRootPath ?? string.Empty;

    /// <summary>
    ///     The path to the config directory.
    /// </summary>
    public static string ConfigPath => MainInstance?.ConfigPath ?? string.Empty;

    /// <summary>
    ///     The path to the global BepInEx configuration file.
    /// </summary>
    public static string BepInExConfigPath => MainInstance?.BepInExConfigPath ?? string.Empty;

    /// <summary>
    ///     The path to temporary cache files.
    /// </summary>
    public static string CachePath => MainInstance?.CachePath ?? string.Empty;


    /// <summary>
    ///     The path to the plugin folder which resides in the BepInEx folder.
    ///     <para>
    ///         This is ONLY guaranteed to be set correctly when Chainloader has been initialized.
    ///     </para>
    /// </summary>
    public static string PluginPath => MainInstance?.PluginPath ?? string.Empty;

    /// <summary>
    ///     The name of the currently executing process.
    /// </summary>
    public static string ProcessName => MainInstance?.ProcessName ?? string.Empty;

}
