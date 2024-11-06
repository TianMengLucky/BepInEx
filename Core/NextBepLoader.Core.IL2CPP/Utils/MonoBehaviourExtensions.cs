using System.Collections;
using NextBepLoader.Core.IL2CPP.Utils.Collections;
using UnityEngine;

namespace NextBepLoader.Core.IL2CPP.Utils;

public static class MonoBehaviourExtensions
{
    public static Coroutine StartCoroutine(this MonoBehaviour self, IEnumerator coroutine) =>
        self.StartCoroutine(coroutine.WrapToIl2Cpp());
}
