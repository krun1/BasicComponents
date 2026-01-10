using BasicComponents.Collections;

namespace BasicComponents.Dispose;

public class DisposeTokenSource : IDisposable
{
    private bool _isDisposed = false;
    internal readonly List<Action> Disposables = new();

    public DisposeTokenSource()
    {
        Token = new DisposeToken(this);
    }

    public DisposeToken Token { get; }
    
    public void Dispose()
    {
        if (_isDisposed)
            return;
        Disposables.AsEnumerable().Reverse().ForAll(a => a());
        _isDisposed = true;
    }
}

public class DisposeToken(DisposeTokenSource source)
{
    public event Action OnDispose
    {
        add => source.Disposables.Add(value);
        remove => source.Disposables.Remove(value);
    }

    public void Register(IDisposable disposable)
        => source.Disposables.Add(disposable.Dispose);
}