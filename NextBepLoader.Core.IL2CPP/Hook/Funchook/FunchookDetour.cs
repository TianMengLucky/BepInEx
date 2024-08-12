using System;
using System.Runtime.CompilerServices;

namespace NextBepLoader.Core.IL2CPP.Hook.Funchook;

internal class FunchookDetour(IntPtr originalMethodPtr, IntPtr detourMethod)
    : BaseNativeDetour<FunchookDetour>(originalMethodPtr, detourMethod)
{
    private readonly nint funchookInstance = FunchookLib.Create();

    protected override void ApplyImpl() => EnsureSuccess(FunchookLib.Install(funchookInstance, 0));

    protected override unsafe void PrepareImpl()
    {
        var trampolinePtr = Source;
        EnsureSuccess(FunchookLib.Prepare(funchookInstance, &trampolinePtr, Target));
        OrigEntrypoint = trampolinePtr;
        HasOrigEntrypoint = true;
    }

    protected override void UndoImpl() => EnsureSuccess(FunchookLib.Uninstall(funchookInstance, 0));

    protected override void FreeImpl() => EnsureSuccess(FunchookLib.Destroy(funchookInstance));

    private string GetErrorMessage()
        => FunchookLib.ErrorMessage(funchookInstance);

    private void EnsureSuccess(FunchookResult result, [CallerArgumentExpression("result")] string methodName = null)
    {
        if (result == FunchookResult.Success) return;
        var errorMsg = GetErrorMessage();
        if (result == FunchookResult.OutOfMemory) throw new OutOfMemoryException($"{methodName} failed: {errorMsg}");
        throw new Exception($"{methodName} failed with result {result}: {errorMsg}");
    }
}
