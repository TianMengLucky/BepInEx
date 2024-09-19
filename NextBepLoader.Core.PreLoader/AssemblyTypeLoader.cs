using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NextBepLoader.Core.PreLoader;

public class AssemblyTypeLoader
{
    public List<Assembly> Assemblies { get; set; } = [];
    public Task? _Task { get; private set; }

    public Dictionary<Assembly, List<Type>> _LoadTypes = [];
    public Action<Type, Assembly>? OnLoadType { get; set; } = null;

    public AssemblyTypeLoader AddAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }

    public AssemblyTypeLoader AddAssemblies(IEnumerable<Assembly> assemblies)
    {
        Assemblies.AddRange(assemblies);
        return this;
    }

    public AssemblyTypeLoader AddAssembliesFormPath(string path)
    {
        Assemblies.Add(Assembly.LoadFile(path));
        return this;
    }
    
    public AssemblyTypeLoader AddAssembliesFormBytes(byte[] bytes)
    {
        Assemblies.Add(Assembly.Load(bytes));
        return this;
    }
    
    public AssemblyTypeLoader AddAssembliesFormDirectory(DirectoryInfo directory)
    {
        foreach (var file in directory.GetFiles(".dll", SearchOption.AllDirectories))
            AddAssembliesFormPath(file.FullName);
        
        return this;
    }
    
    public AssemblyTypeLoader StartNewLoad()
    {
        _Task ??= Task.Factory.StartNew(LoadTypes);
        return this;
    }

    private void LoadTypes()
    {
        foreach (var assembly in Assemblies)
        {
            if (!_LoadTypes.ContainsKey(assembly))
                _LoadTypes.Add(assembly, []);

            var list = _LoadTypes[assembly];
            var types = assembly.GetTypes();
            foreach (var variableType in types)
            {
                list.Add(variableType);
                OnLoadType?.Invoke(variableType, assembly);
            }
        }
    }
}
