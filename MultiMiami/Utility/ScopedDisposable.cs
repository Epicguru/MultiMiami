namespace MultiMiami.Utility;

public readonly struct ScopedDisposable<T> : IDisposable
{
    public readonly T Payload;
    public readonly Action<T> DisposeAction;

    public ScopedDisposable(in T payload, Action<T> dispose)
    {
        Payload = payload;
        DisposeAction = dispose;
    }

    public void Dispose()
    {
        DisposeAction(Payload);
    }

    public static implicit operator T(in ScopedDisposable<T> p) => p.Payload;
}