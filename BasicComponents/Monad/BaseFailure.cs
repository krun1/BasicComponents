using System.Collections.Immutable;

namespace BasicComponents.Monad;

public abstract class BaseFailure
{
    public abstract string Message { get; }
    public abstract Exception ToException();
    public virtual bool IsHandled => false;
}

public sealed class ExceptionFailure(Exception e) : BaseFailure
{
    public override string Message => e.Message;
    public override Exception ToException() => e;
}

public class MessageFailure(string s) : BaseFailure
{
    public override string Message => s;
    public override Exception ToException() => new(Message);
}

public sealed class ValueFailure<T>(T value) : BaseFailure
{
    public override string Message => $"Invalid value {value}";
    public override Exception ToException() => new(Message);
}

public class HandledFailure(BaseFailure inner) : BaseFailure
{
    public override bool IsHandled => true;
    public override string Message => inner.Message;
    public override Exception ToException() => inner.ToException();
}

public class AggregateFailure(IEnumerable<BaseFailure> inner) : BaseFailure
{
    public override bool IsHandled => Inner.All(f => f.IsHandled);

    public override string Message
        => $"{nameof(AggregateFailure)}\n{string.Join(Environment.NewLine, Inner.Select(failure => failure.Message))}";

    public ImmutableArray<BaseFailure> Inner { get; } = [..inner];

    public override Exception ToException() => new AggregateException(Inner.Select(f => f.ToException()));
}

public static class FailureExtension
{
    public static T? Cast<T>(this BaseFailure s) where T : BaseFailure
    {
        if (s.IsHandled)
            return null;
        if (s is T t)
            return t;
        if (s is AggregateFailure ae)
        {
            foreach (var a in ae.Inner)
            {
                if (a.Cast<T>() is { } r)
                    return r;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Replaces the unhandled failure of type <typeparamref name="T"/> - or each of them, inside an
    /// aggregate - with what <paramref name="func"/> makes of it. Any other failure is kept as is.
    /// Should <paramref name="func"/> throw, the exception is added to the failure it was mapping.
    /// </summary>
    public static BaseFailure Map<T>(this BaseFailure self, Func<T, BaseFailure> func) where T : BaseFailure
    {
        if (self.IsHandled)
            return self;
        if (self is T t)
        {
            try
            {
                return func(t);
            }
            catch (Exception e)
            {
                return self.Concat(new ExceptionFailure(e));
            }
        }
        if (self is AggregateFailure af)
            return new AggregateFailure(af.Inner.Select(f => f.Map(func)));
        return self;
    }

    public static BaseFailure Concat(this BaseFailure l, BaseFailure r)
    {
        List<BaseFailure> result = [];

        if (l is AggregateFailure al)
            result.AddRange(al.Inner);
        else
            result.Add(l);
        if (r is AggregateFailure ar)
            result.AddRange(ar.Inner);
        else
            result.Add(r);
        return new AggregateFailure(result);
    }

    public static BaseFailure Handle(this BaseFailure self, BaseFailure handled)
    {
        if (handled.IsHandled)
            return handled;
        if (ReferenceEquals(self, handled))
            return new HandledFailure(self);
        if (self is AggregateFailure af && af.Inner.Contains(handled)) 
            return new AggregateFailure(af.Inner.Replace(handled, new HandledFailure(handled)));
        return new HandledFailure(handled);
    }
    
    public static IEnumerable<BaseFailure> Flatten(this BaseFailure l)
    {
        if (l.IsHandled)
            yield break;
        if (l is AggregateFailure af)
        {
            foreach (var a in af.Inner)
                yield return a;
        }
        else
        {
            yield return l;
        }
    }
}