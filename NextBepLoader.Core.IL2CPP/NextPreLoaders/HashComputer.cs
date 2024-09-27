using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cpp2IL.Core;
using Il2CppInterop.Generator.Runners;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class HashComputer(INextBepEnv env, ILogger<HashComputer> logger) : BasePreLoader
{
    public override Type[] WaitLoadLoader => [typeof(IL2CPPPreLoader)];
    public IL2CPPCheckEventArg EventArg;
    private static string HashPath => Path.Combine(Paths.CacheDataDir, "InteropAssembly.Hash");
    private static string AssemblyHashPath => Path.Combine(Paths.CacheDataDir, "GameAssembly.Hash");
    public string CurrentHashString { get; private set; }

    public override void PreLoad(PreLoadEventArg arg)
    {
        EventArg = env.GetOrCreateEventArgs<IL2CPPCheckEventArg>();
        EventArg.UpdateIL2CPPInteropAssembly = CheckIfGenerationRequired();
        EventArg.UpdateCPP2ILAssembly = File.ReadAllText(AssemblyHashPath) != ComputeGameAssemblyHash();
    }

    public override void Finish()
    {
        if (!EventArg.UpdateIL2CPPInteropAssembly) return;
        WriteComputeHash();
    }
    
    private bool CheckIfGenerationRequired()
    {
        ComputeHash();
        
        if (EventArg.UpdateIL2CPPInteropAssembly) return true;

        if (!File.Exists(HashPath))
            return true;
        
        if (CurrentHashString == File.ReadAllText(HashPath)) 
            return false;
        
        logger.LogInformation("Detected outdated interop assemblies, will regenerate them now");
        return true;
    }
    

    public void WriteComputeHash() => File.WriteAllText(HashPath, ComputeHash());

    public void WriteAssemblyHash() => File.WriteAllText(AssemblyHashPath, ComputeGameAssemblyHash());

    private string ComputeGameAssemblyHash()
    {
        using var md5 = MD5.Create();
        md5.HashFile(Paths.GameAssemblyPath);
        md5.HashFile(Paths.GameMetaDataPath);
        return Utility.ByteArrayToString(md5.Hash!);
    }
    
    private string ComputeHash()
    {
        using var md5 = MD5.Create();
        md5.HashFile(Paths.GameAssemblyPath);
        if (Directory.Exists(Paths.UnityBaseDirectory))
            foreach (var file in Directory.EnumerateFiles(Paths.UnityBaseDirectory, "*.dll", SearchOption.TopDirectoryOnly))
                md5.HashFile(file);

        // Hash some common dependencies as they can affect output
        md5.HashString(typeof(InteropAssemblyGenerator).Assembly.GetName().Version?.ToString()!);
        md5.HashString(typeof(Cpp2IlApi).Assembly.GetName().Version?.ToString()!);
        md5.TransformFinalBlock([], 0, 0);
        return CurrentHashString = Utility.ByteArrayToString(md5.Hash!);
    }
}
