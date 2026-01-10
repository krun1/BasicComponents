namespace BasicComponents;

public sealed class ActionOnDispose(Action action) : IDisposable
{
    public void Dispose() => action();
}