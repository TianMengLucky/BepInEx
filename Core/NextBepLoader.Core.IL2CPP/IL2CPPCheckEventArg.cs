using System;
using System.Collections.Generic;
using AsmResolver.DotNet;

namespace NextBepLoader.Core.IL2CPP;

public class IL2CPPCheckEventArg : EventArgs
{
    public bool DownloadUnityBaseLib { get; set; }
    public bool UpdateIL2CPPInteropAssembly { get; set; }
    public bool CacheCPP2ILAssembly { get; set; }
    public List<AssemblyDefinition> ResolverAssemblies { get; set; } = [];
}
