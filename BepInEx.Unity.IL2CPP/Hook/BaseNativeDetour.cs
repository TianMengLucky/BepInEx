using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using MonoMod.Core;
using MonoMod.RuntimeDetour;

namespace BepInEx.Unity.IL2CPP.Hook;

internal abstract class BaseNativeDetour<T>(nint originalMethodPtr, Delegate detourMethod) : INativeDetour
    where T : BaseNativeDetour<T>
{
    protected static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(typeof(T).Name);

    public bool IsPrepared { get; protected set; }
    protected MethodInfo TrampolineMethod { get; set; }
    protected Delegate DetourMethod { get; set; } = detourMethod;

    public nint OriginalMethodPtr { get; } = originalMethodPtr;
    public nint DetourMethodPtr { get; } = Marshal.GetFunctionPointerForDelegate(detourMethod);
    public nint TrampolinePtr { get; protected set; }
    public bool IsValid { get; private set; } = true;
    public bool IsApplied { get; private set; }

    public void Dispose()
    {
        if (!IsValid) return;
        Undo();
        Free();
    }

    public void Apply()
    {
        if (IsApplied) return;

        Prepare();
        ApplyImpl();

        Logger.Log(LogLevel.Debug,
                   $"Original: {OriginalMethodPtr:X}, Trampoline: {TrampolinePtr:X}, diff: {Math.Abs(OriginalMethodPtr - TrampolinePtr):X}");

        IsApplied = true;
    }

    public void Undo()
    {
        if (IsApplied && IsPrepared) UndoImpl();
    }

    public void Free()
    {
        FreeImpl();
        IsValid = false;
    }

    public MethodBase GenerateTrampoline(MethodBase signature = null)
    {
        if (TrampolineMethod == null)
        {
            Prepare();
            TrampolineMethod = n.GenerateNativeProxy(TrampolinePtr, signature);
        }

        return TrampolineMethod;
    }

    public TDelegate GenerateTrampoline<TDelegate>() where TDelegate : Delegate
    {
        if (!typeof(Delegate).IsAssignableFrom(typeof(TDelegate)))
            throw new InvalidOperationException($"Type {typeof(TDelegate)} not a delegate type.");

        _ = GenerateTrampoline(typeof(TDelegate).GetMethod("Invoke"));

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(TrampolinePtr);
    }

    public nint Target { get; }
    public nint Detour { get; }
    public nint OriginalTrampoline { get; }

    protected abstract void ApplyImpl();

    private void Prepare()
    {
        if (IsPrepared) return;
        Logger.LogDebug($"Preparing detour from 0x{OriginalMethodPtr:X2} to 0x{DetourMethodPtr:X2}");
        PrepareImpl();
        Logger.LogDebug($"Prepared detour; Trampoline: 0x{TrampolinePtr:X2}");
        IsPrepared = true;
    }

    protected abstract void PrepareImpl();

    protected abstract void UndoImpl();

    protected abstract void FreeImpl();
    public IDetourFactory Factory { get; }
    public DetourConfig Config { get; }
    public MethodInfo PublicTarget { get; }
    public MethodInfo InvokeTarget { get; }
}
