using System.IO;

namespace NextBepLoader.Core;

public abstract class LoaderPathBase
{
    /// <summary>
    ///     The directory that the currently executing process resides in.
    ///     <para>On OSX however, this is the parent directory of the game.app folder.</para>
    /// </summary>
    public abstract string GameRootPath { get; set; }
    
    /// <summary>
    ///     The path to the main BepInEx folder.
    /// </summary>
    public string BepInExRootPath { get; set; }
    
    /// <summary>
    ///     The path to the Managed folder that contains the main managed assemblies.
    /// </summary>
    public string ManagedPath { get;  set; }

    /// <summary>
    ///     The path to the game data folder of the currently running Unity game.
    /// </summary>
    public string GameDataPath { get; set; }

    /// <summary>
    ///     The directory that the core BepInEx DLLs reside in.
    /// </summary>
    public string BepInExAssemblyDirectory { get;  set; }

    /// <summary>
    ///     The path to the core BepInEx DLL.
    /// </summary>
    public string BepInExAssemblyPath { get; set; }

    /// <summary>
    ///     The path of the currently executing program BepInEx is encapsulated in.
    /// </summary>
    public string? ExecutablePath { get; set; }
    

    /// <summary>
    ///     The path to the config directory.
    /// </summary>
    public  string ConfigPath { get;  set; }

    /// <summary>
    ///     The path to the global BepInEx configuration file.
    /// </summary>
    public string BepInExConfigPath { get; set; }

    /// <summary>
    ///     The path to temporary cache files.
    /// </summary>
    public string CachePath { get;  set; }


    /// <summary>
    ///     The path to the plugin folder which resides in the BepInEx folder.
    ///     <para>
    ///         This is ONLY guaranteed to be set correctly when Chainloader has been initialized.
    ///     </para>
    /// </summary>
    public string PluginPath { get;  set; }

    /// <summary>
    ///     The name of the currently executing process.
    /// </summary>
    public string ProcessName { get;  set; }

    /// <summary>
    ///     List of directories from where Mono will search assemblies before assembly resolving is invoked.
    /// </summary>
    public string[] DllSearchPaths { get; set; }


    public virtual void InitPaths()
    {
        Paths.MainInstance = this;
    }
}
