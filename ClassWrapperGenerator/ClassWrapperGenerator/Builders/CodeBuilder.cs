using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ClassWrapperGenerator;

public class CodeBuilder(int baseScope = 0)
{
    private StringBuilder _sb = new();
    private int scope = baseScope;
    private bool _isNewLine = true;
    public const string NewLine = "\r\n";
    
    private string Indent => string.Concat(Enumerable.Repeat(' ', scope * 4));
    
    public void Append(string s)
    {
        _sb.Append(AddIndent(s));
        _isNewLine = s.EndsWith(NewLine);
    }

    public void AppendLine() => _sb.AppendLine();
    
    public void AppendLine(string s)
    {
        _sb.AppendLine(AddIndent(s));
        _isNewLine = true;
    }

    public void AddScope() => scope++;
    public void RemoveScope() => scope = Math.Max(scope - 1, 0);

    public IDisposable Scope()
    {
        AddScope();
        return new DisposableAction(RemoveScope);
    }
    
    public IDisposable Scope(string startWith, string endWith)
    {
        AppendLine(startWith);
        AddScope();
        return new DisposableAction(() =>
        {
            RemoveScope();
            AppendLine(endWith);
        });
    }
    
    private string AddIndent(string s)
    {
        StringBuilder sb = new(s.Length);
        if (_isNewLine)
            sb.Append(Indent);
        sb.Append(s.Replace(NewLine, NewLine + Indent));
        
        return sb.ToString();
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}

internal class DisposableAction(Action action) : IDisposable
{
    public void Dispose()
    {
        action.Invoke();
    }
}