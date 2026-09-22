using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace BasicComponents.Monad;

public abstract class BaseFailure
{
    private readonly StackTrace? _stackTrace;

    protected BaseFailure() : this(captureStackTrace: true)
    {
    }

    private protected BaseFailure(bool captureStackTrace)
    {
        if (captureStackTrace)
            _stackTrace = Capture();
    }

    /// <summary>
    /// The stack the failure was created from - its own constructors and the factories of this library
    /// left out.
    /// </summary>
    public virtual StackTrace? StackTrace => _stackTrace;

    public abstract string Message { get; }
    public abstract Exception ToException();
    public virtual bool IsHandled => false;

    /// <summary>
    /// Gives <paramref name="exception"/>, never thrown, the <see cref="StackTrace"/> of this failure,
    /// so that logging it shows where the failure came from.
    /// </summary>
    protected Exception WithStackTrace(Exception exception)
    {
        if (StackTrace is { FrameCount: > 0 } stackTrace && exception.StackTrace == null)
        {
            try
            {
                ExceptionDispatchInfo.SetRemoteStackTrace(exception, stackTrace.ToString());
            }
            catch (InvalidOperationException)
            {
                // thrown or given a stack trace meanwhile: it already tells where it comes from
            }
        }
        return exception;
    }

    private static StackTrace Capture()
    {
        var frames = new StackTrace(1, fNeedFileInfo: true).GetFrames();
        var first = Array.FindIndex(frames, f => !IsLibraryFrame(f.GetMethod()));
        return new StackTrace(first < 0 ? frames : frames[first..]);
    }

    private static bool IsLibraryFrame(MethodBase? method)
        => method?.DeclaringType is { } type
           && (type.Assembly == typeof(BaseFailure).Assembly
               || (method.IsConstructor && typeof(BaseFailure).IsAssignableFrom(type)));
}

public sealed class ExceptionFailure(Exception e) : BaseFailure(captureStackTrace: e.StackTrace == null)
{
    public Exception Exception => e;
    public override string Message => e.Message;

    /// <summary>Where <see cref="Exception"/> was thrown, or else where the failure was created.</summary>
    public override StackTrace? StackTrace => e.StackTrace != null ? new StackTrace(e, fNeedFileInfo: true) : base.StackTrace;

    public override Exception ToException() => WithStackTrace(e);
}

public class MessageFailure(string s) : BaseFailure
{
    public override string Message => s;
    public override Exception ToException() => WithStackTrace(new Exception(Message));
}

public sealed class ValueFailure<T>(T value) : BaseFailure
{
    public override string Message => $"Invalid value {value}";
    public override Exception ToException() => WithStackTrace(new Exception(Message));
}

public class HandledFailure(BaseFailure inner) : BaseFailure(captureStackTrace: false)
{
    public override bool IsHandled => true;
    public override string Message => inner.Message;
    public override StackTrace? StackTrace => inner.StackTrace;
    public override Exception ToException() => inner.ToException();
}

/// <summary>Has no <see cref="BaseFailure.StackTrace"/> of its own: each of <see cref="Inner"/> keeps its.</summary>
public class AggregateFailure(IEnumerable<BaseFailure> inner) : BaseFailure(captureStackTrace: false)
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
        => s.Find<T>(_ => true);

    /// <summary>
    /// Finds the unhandled <see cref="ExceptionFailure"/> whose exception is a <typeparamref name="TException"/>,
    /// or derives from it - looking inside aggregates too.
    /// </summary>
    public static ExceptionFailure? CastException<TException>(this BaseFailure s) where TException : Exception
        => s.Find<ExceptionFailure>(f => f.Exception is TException);

    private static T? Find<T>(this BaseFailure s, Func<T, bool> predicate) where T : BaseFailure
    {
        if (s.IsHandled)
            return null;
        if (s is T t && predicate(t))
            return t;
        if (s is AggregateFailure ae)
        {
            foreach (var a in ae.Inner)
            {
                if (a.Find(predicate) is { } r)
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