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
}