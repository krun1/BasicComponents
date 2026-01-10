namespace BasicComponents;

public class ReentrancyScope(bool baseValue)
{
    private int _scope = 0;
    
    public bool Value => _scope == 0 ? baseValue : !baseValue;
    
    public IDisposable Scope()
    {
        Interlocked.Increment(ref _scope);
        return new ActionOnDispose(() => Interlocked.Decrement(ref _scope));
    }
}