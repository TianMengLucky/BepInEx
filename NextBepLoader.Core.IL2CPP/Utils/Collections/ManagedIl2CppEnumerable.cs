using System;
using Il2CppSystem.Collections;
using IEnumerator = System.Collections.IEnumerator;

namespace NextBepLoader.Core.IL2CPP.Utils.Collections;

public class ManagedIl2CppEnumerable(IEnumerable enumerable) : System.Collections.IEnumerable
{
    private readonly IEnumerable enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));

    public IEnumerator GetEnumerator() => new ManagedIl2CppEnumerator(enumerable.GetEnumerator());
}
