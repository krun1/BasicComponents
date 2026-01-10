using System.Collections.Generic;
using System.Text;

namespace ClassWrapperGenerator;

public class ClassBuilder(string namespaceName, string className)
{
    private HashSet<string> _usings = [];
    private List<string> _fields = [];
    private List<string> _functions = [];
    private List<string> _properties = [];

    public StringBuilder Header { get; } = new();
    public bool IsPartial { get; set; }
    
    public bool AddUsing(string newUsing) => _usings.Add(newUsing);
    public void AddField(string field) => _fields.Add(field);
    public void AddFunction(string function) => _functions.Add(function);
    public void AddProperty(string property) => _properties.Add(property);

    public override string ToString()
    {
        var cd = new CodeBuilder();

        cd.AppendLine(Header.ToString());
        foreach (var u in _usings) cd.AppendLine($"using {u};");

        cd.AppendLine();
        cd.AppendLine($"namespace {namespaceName};");
        cd.AppendLine();

        cd.Append("public ");
        if (IsPartial)
            cd.Append("partial ");
        cd.AppendLine($"class {className}");

        using (cd.Scope("{", "}"))
        {
            foreach (var field in _fields) cd.AppendLine(field);
            foreach (var property in _properties) cd.AppendLine(property);
            foreach (var function in _functions) cd.AppendLine(function);
        }
        return cd.ToString();
    }

}