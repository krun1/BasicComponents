using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using ClassWrapper;

namespace ClassWrapperGenerator.Sample;

[Extend(".*myjson.json")]
[ToString]
// [MockFromJson(typeof(ITestLog))]
public partial class ExtendTest2
{
    [Async]
    public void NotAsync()
    {
    }
    
    [Async]
    public void NotAsync(int i)
    {
    }
    
    [Async]
    public void NotAsync(int i, string j)
    {
    }
    
    [Async]
    public int NotAsyncWithReturn(int i, int j)
    {
        return i + j;
    }

    [SetField]
    public partial void SetToto(int toto);

    [GetField]
    public partial int GetToto();

    // [LoadJsonFile("J")]
    // public partial StateData LoadFromFile(string filePath);
    //
    // [LoadJsonFile]
    // public partial StateData LoadFromObject(JsonNode node);
    //
    public ClassWrapperGenerator.Sample.StateData test(string filePath)
    {
        var o = JsonNode.Parse(filePath);
        
        var res = new ClassWrapperGenerator.Sample.StateData
        {
            I = o["I"].GetValue<int>(),
            J = o["J"].GetValue<bool>(),
            rI = o["rI"].AsArray().Select(n => n.GetValue<int>()).ToArray(),
        };
        return res;
    }
}

public class StateData
{
    public int I { get; set; }
    public bool J { get; set; }
    public string Str { get; set; }
    public MyEnum Enum { get; set; }
    public Other O { get; set; }
    public int[] rI { get; set; }
    public List<int> lI { get; set; }
    public IList<int> IlI { get; set; }
    public IReadOnlyList<int> IRolI { get; set; }
    public IReadOnlyList<Other> lo { get; set; }

    public enum MyEnum { A, B, C }
    public class Other
    {
        public int i { get; set; }
        public int j { get; set; }
        public int k { get; set; }
        public int? N { get; set; }
        public Guid Gid { get; set; }
    }
}
