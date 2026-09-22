using System.Runtime.CompilerServices;
using BasicComponents.Monad;

namespace BasicComponentsTest;

public class TestFailureStackTrace
{
    class TestError(string s) : MessageFailure(s) {}

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BaseFailure CreateMessageFailure() => new MessageFailure("failure");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BaseFailure CreateTestError() => new TestError("failure");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static DataOrError<int> CreateThroughFactory() => DataOrError.Error<int>("failure");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BaseFailure CreateUnthrownExceptionFailure() => new ExceptionFailure(new InvalidOperationException("boom"));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Throw() => throw new InvalidOperationException("boom");

    private static string? FirstMethod(BaseFailure failure) => failure.StackTrace?.GetFrame(0)?.GetMethod()?.Name;

    [Test]
    public void TestStackTraceStartsAtCreation()
    {
        Assert.That(FirstMethod(CreateMessageFailure()), Is.EqualTo(nameof(CreateMessageFailure)));
    }

    [Test]
    public void TestStackTraceSkipsDerivedConstructors()
    {
        Assert.That(FirstMethod(CreateTestError()), Is.EqualTo(nameof(CreateTestError)));
    }

    [Test]
    public void TestStackTraceSkipsLibraryFactories()
    {
        Assert.That(FirstMethod(CreateThroughFactory().Failure), Is.EqualTo(nameof(CreateThroughFactory)));
    }

    [Test]
    public void TestToExceptionCarriesStackTrace()
    {
        var exception = CreateMessageFailure().ToException();

        Assert.That(exception.StackTrace, Does.Contain(nameof(CreateMessageFailure)));
    }

    [Test]
    public void TestThrownExceptionKeepsItsStackTrace()
    {
        var failure = DataOrError.Try<int>(() =>
        {
            Throw();
            return 0;
        }).Failure;

        Assert.That(FirstMethod(failure), Is.EqualTo(nameof(Throw)));
        Assert.That(failure.ToException().StackTrace, Does.Contain(nameof(Throw)));
    }

    [Test]
    public void TestUnthrownExceptionGetsCreationStackTrace()
    {
        var failure = CreateUnthrownExceptionFailure();

        Assert.That(FirstMethod(failure), Is.EqualTo(nameof(CreateUnthrownExceptionFailure)));
        Assert.That(failure.ToException().StackTrace, Does.Contain(nameof(CreateUnthrownExceptionFailure)));
    }

    [Test]
    public void TestHandledFailureKeepsInnerStackTrace()
    {
        var check = Check.Fail(CreateMessageFailure()).OnError((MessageFailure _) => { });

        Assert.That(check.Failure.IsHandled, Is.True);
        Assert.That(FirstMethod(check.Failure), Is.EqualTo(nameof(CreateMessageFailure)));
    }

    [Test]
    public void TestAggregateKeepsEachStackTrace()
    {
        var aggregate = CreateMessageFailure().Concat(CreateTestError());

        Assert.That(aggregate.StackTrace, Is.Null);
        Assert.That(aggregate.Flatten().Select(FirstMethod),
            Is.EqualTo(new[] { nameof(CreateMessageFailure), nameof(CreateTestError) }));
    }
}
