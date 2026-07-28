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
}