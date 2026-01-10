using ClassWrapper;

namespace ClassWrapperGenerator.Sample;

[LogWrapper]
public interface ITestLog
{
    int I { get; }
    int J { get; set; }
    
    void DoSomething();
    int GetSomeNumber();
}
