using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using AssemblyDefinition = AsmResolver.DotNet.AssemblyDefinition;

namespace NextBepLoader.Core.IL2CPP;

internal static partial class Il2CppInteropManager
{
    private sealed class AsmToCecilConverter : IAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> asmResolverDictionary;
        private readonly Dictionary<AssemblyDefinition, Mono.Cecil.AssemblyDefinition> asmToCecil = new();
        private readonly Dictionary<string, Mono.Cecil.AssemblyDefinition> cecilDictionary = new();

        public AsmToCecilConverter(List<AssemblyDefinition> list)
        {
            asmResolverDictionary = list.ToDictionary(a => a.Name!.ToString(), a => a)!;
        }

        public void Dispose() { }

        public Mono.Cecil.AssemblyDefinition? Resolve(AssemblyNameReference name) =>
            Resolve(name, new ReaderParameters { AssemblyResolver = this });

        public Mono.Cecil.AssemblyDefinition? Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            var assemblyName = name.Name;
            if (!cecilDictionary.TryGetValue(assemblyName, out var cecilAssembly) &&
                asmResolverDictionary.TryGetValue(assemblyName, out var asmAssembly))
                cecilAssembly = Convert(asmAssembly, parameters);
            return cecilAssembly;
        }

        public List<Mono.Cecil.AssemblyDefinition> ConvertAll()
        {
            List<Mono.Cecil.AssemblyDefinition> cecilAssemblies = new(asmResolverDictionary.Count);
            cecilAssemblies.AddRange(asmResolverDictionary.Values.Select(Convert));

            return cecilAssemblies;
        }

        private Mono.Cecil.AssemblyDefinition Convert(AssemblyDefinition asmResolverAssembly) =>
            Convert(asmResolverAssembly, new ReaderParameters { AssemblyResolver = this });

        private Mono.Cecil.AssemblyDefinition Convert(AssemblyDefinition asmResolverAssembly,
                                                      ReaderParameters readerParameters)
        {
            if (asmToCecil.TryGetValue(asmResolverAssembly, out var cecilAssembly)) return cecilAssembly;
            MemoryStream stream = new();
            asmResolverAssembly.WriteManifest(stream);
            stream.Position = 0;
            cecilAssembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(stream, readerParameters);
            cecilDictionary.Add(cecilAssembly.Name.Name, cecilAssembly);
            asmToCecil.Add(asmResolverAssembly, cecilAssembly);
            return cecilAssembly;
        }
    }
}
