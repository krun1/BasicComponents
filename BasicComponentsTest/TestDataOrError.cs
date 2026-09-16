using BasicComponents.Monad;

namespace BasicComponentsTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestDataOrErrorSelectAsync()
    {
        Task<DataOrError<int>> async = DataOrError.TryAsync(() => Task.FromResult(1))
            .SelectAsync(i => i);
        Task<DataOrError<int>> selectAsync = async
            .SelectAsync(i => Task.FromResult(i + 1));
        
        Assert.ThatAsync(async () => (await async).IsValid, Is.EqualTo(true));
        Assert.ThatAsync(async () => (await selectAsync).IsValid, Is.EqualTo(true));
        Assert.ThatAsync(async () => (await async).Value, Is.EqualTo(1));
        Assert.ThatAsync(async () => (await selectAsync).Value, Is.EqualTo(2));
    }
    
    
    [Test]
    public void TestDataOrErrorThenAsync()
    {
        Task<DataOrError<int>> async = DataOrError.TryAsync(() => Task.FromResult(1))
            .ThenAsync(DataOrError.Create);
        Task<DataOrError<int>> selectAsync = DataOrError.TryAsync(() => Task.FromResult(1))
            .ThenAsync(i => Task.FromResult(DataOrError.Create(i + 1)));
        
        Assert.ThatAsync(async () => (await async).IsValid, Is.EqualTo(true));
        Assert.ThatAsync(async () => (await selectAsync).IsValid, Is.EqualTo(true));
        Assert.ThatAsync(async () => (await async).Value, Is.EqualTo(1));
        Assert.ThatAsync(async () => (await selectAsync).Value, Is.EqualTo(2));
    }

        class TestError(string s) : MessageFailure(s) {}
    
    [Test]
    public void TestFlowSelectError()
    {
        var i = DataOrError.Error<int>(new TestError("failure"))
            .OnError((TestError e) =>
            {
                Assert.That(e.Message, Is.EqualTo("failure"));
                return -1;
            })
            .Select(str => 1);

        Assert.That(i.Result.Value, Is.EqualTo(-1));
    }
    
    [Test]
    public void TestFlowSelectSuccess()
    {
        var i = DataOrError.Create(5)
            .OnError((TestError e) =>
            {
                Assert.That(e.Message, Is.EqualTo("failure"));
                return -1;
            })
            .Select(i => i + 1);

        Assert.That(i.Resolve(i1 => i1), Is.EqualTo(6));
    }

    [Test]
    public void TestFlowThen()
    {
        var i = DataOrError.Create(5)
            .OnError((TestError e) =>
            {
                Assert.That(e.Message, Is.EqualTo("failure"));
                return -1;
            })
            .Then(i => DataOrError.Error<string>(new TestError("error")))
            .OnError((TestError e) =>
            {
                Assert.That(e.Message, Is.EqualTo("error"));
                return -2;
            });

        Assert.That(i.Result.Value, Is.EqualTo(-2));
    }
    
    [Test]
    public void TestFlowMultipleError()
    {
        var i = DataOrError.Error<int>(new TestError("failure"))
            .OnError((TestError e) =>
            {
                Assert.That(e.Message, Is.EqualTo("failure"));
                return -1;
            })
            .Then(i => DataOrError.Error<string>(new TestError("")))
            .OnError((TestError e) =>
            {
                Assert.Fail();
                return -2;
            });

        Assert.That(i.Result.Value, Is.EqualTo(-1));
    }
    
    [Test]
    public void TestFlowMultipleToHandleError()
    {
        var i = DataOrError.Error<int>(new TestError("failure"))
            .OnError((TestError e) =>
            {
                Assert.That(e.Message, Is.EqualTo("failure"));
                return -1;
            })
            .Zip(DataOrError.Error<string>(new TestError("fail2")), (i, s) => (i, s))
            .OnError((TestError e, Maybe<int> prev) =>
            {
                Assert.That(e.Message, Is.EqualTo("fail2"));
                return prev.OrElse(-2);
            });
        Assert.That(i.Result.Value, Is.EqualTo(-1));
    }

    [Test]
    public async Task TestCheckTaskSelectAsync()
    {
        var success = await Task.FromResult(Check.Success())
            .SelectAsync(() => Task.FromResult(1));
        var failure = await Task.FromResult(Check.Fail(new TestError("failure")))
            .SelectAsync(() => Task.FromResult(1));

        Assert.That(success.Value, Is.EqualTo(1));
        Assert.That(failure.Failure, Is.TypeOf<TestError>());
    }

    class DerivedTestError(string s) : TestError(s) {}

    [Test]
    public void TestFlowHandlesDerivedError()
    {
        var i = DataOrError.Error<int>(new DerivedTestError("failure"))
            .OnError((TestError e) => -1);

        Assert.That(i.Resolve(v => v), Is.EqualTo(-1));
    }

    [Test]
    public async Task TestFlowHandledErrorSurvivesAsyncSteps()
    {
        var step = await DataOrError.Error<int>(new Exception("failure"))
            .OnError((ExceptionFailure e) => -1)
            .ThenAsync(i => Task.FromResult(DataOrError.Create(i + 1)));
        step = await step
            .OnError((ExceptionFailure e) =>
            {
                Assert.Fail();
                return -2;
            })
            .SelectAsync(i => Task.FromResult(i + 1));

        Assert.That(step.Resolve(i => i, e => -3), Is.EqualTo(-1));
    }

    class OtherTestError(string s) : MessageFailure(s) {}

    [Test]
    public void TestMapFailureReplacesMatchingFailure()
    {
        var mapped = DataOrError.Error<int>(new DerivedTestError("failure"))
            .MapFailure((TestError e) => new OtherTestError($"mapped {e.Message}"));

        Assert.That(mapped.Failure, Is.TypeOf<OtherTestError>());
        Assert.That(mapped.Failure.Message, Is.EqualTo("mapped failure"));
    }

    [Test]
    public void TestMapFailureKeepsOtherFailuresAndValues()
    {
        var other = DataOrError.Error<int>(new OtherTestError("failure"))
            .MapFailure((TestError e) => new MessageFailure("mapped"));
        var valid = DataOrError.Create(5)
            .MapFailure((TestError e) => new MessageFailure("mapped"));

        Assert.That(other.Failure, Is.TypeOf<OtherTestError>());
        Assert.That(valid.Value, Is.EqualTo(5));
    }

    [Test]
    public void TestMapFailureInAggregate()
    {
        var mapped = DataOrError.Error<int>(new TestError("first"))
            .Zip(DataOrError.Error<int>(new OtherTestError("second")), (l, r) => l + r)
            .MapFailure((TestError e) => new MessageFailure("mapped"));

        var inner = ((AggregateFailure)mapped.Failure).Inner;
        Assert.That(inner[0], Is.TypeOf<MessageFailure>());
        Assert.That(inner[1], Is.TypeOf<OtherTestError>());
    }

    [Test]
    public void TestMapFailureIgnoresHandledFailure()
    {
        var step = DataOrError.Error<int>(new TestError("failure"))
            .OnError((TestError e) => -1)
            .MapFailure((TestError e) =>
            {
                Assert.Fail();
                return e;
            });

        Assert.That(step.Resolve(i => i), Is.EqualTo(-1));
    }

    [Test]
    public void TestMapFailureThrowingKeepsOriginal()
    {
        var mapped = DataOrError.Error<int>(new TestError("failure"))
            .MapFailure((TestError e) => throw new InvalidOperationException("boom"));

        var inner = ((AggregateFailure)mapped.Failure).Inner;
        Assert.That(inner[0], Is.TypeOf<TestError>());
        Assert.That(inner[1].ToException(), Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task TestCheckMapFailure()
    {
        var mapped = Check.Fail(new TestError("failure"))
            .MapFailure((TestError e) => new OtherTestError("mapped"));
        var mappedAsync = await Task.FromResult(Check.Fail(new TestError("failure")))
            .MapFailureAsync((TestError e) => new OtherTestError("mapped"));
        var dataAsync = await Task.FromResult(DataOrError.Error<int>(new TestError("failure")))
            .MapFailureAsync((TestError e) => new OtherTestError("mapped"));

        Assert.That(mapped.Failure, Is.TypeOf<OtherTestError>());
        Assert.That(mappedAsync.Failure, Is.TypeOf<OtherTestError>());
        Assert.That(dataAsync.Failure, Is.TypeOf<OtherTestError>());
        Assert.That(Check.Success().MapFailure((TestError e) => e).IsValid, Is.True);
    }
}