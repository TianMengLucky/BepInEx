using System;

namespace BepInEx.Unity.IL2CPP.Hook.Dobby;

internal class DobbyDetour(IntPtr originalMethodPtr, IntPtr detourMethod)
    : BaseNativeDetour<DobbyDetour>(originalMethodPtr, detourMethod)
{
    protected override void ApplyImpl() => _ = DobbyLib.Commit(Source);

    protected override unsafe void PrepareImpl()
    {
        nint trampolinePtr = 0;
        _ = DobbyLib.Prepare(Source, Target,  &trampolinePtr);
        OrigEntrypoint = trampolinePtr;
        HasOrigEntrypoint = true;
    }

    protected override void UndoImpl() => _ = DobbyLib.Destroy(Source);

    protected override void FreeImpl() { }
}
