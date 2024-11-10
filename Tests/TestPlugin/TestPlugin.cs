using HarmonyLib;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.Contract.Attributes;


namespace TestPlugin;

[PluginMetadata(LoaderPlatformType.Desktop, "cn.mengchu.testplugin","TestPlugin", "1.0.0")]
public class TestPlugin(ILogger<TestPlugin> logger) : BasePlugin
{
    public readonly Harmony _Harmony = new("cn.mengchu.testplugin");
    public override void Load()
    {
        
        logger.LogInformation("TestPlugin");
        _Harmony.PatchAll();
    }
}

