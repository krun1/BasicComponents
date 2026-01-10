using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace ClassWrapperGenerator.Extendables;

[UsedImplicitly]
public class AsyncExtend : IExtendable
{
    public void ExtendMethod(ClassBuilder cb, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        cb.AddUsing("System.Threading.Tasks");
        if (methodSymbol.ReturnsVoid)
        {
            cb.AddFunction($$"""
                         public Task {{methodSymbol.Name}}Async({{string.Join(", ", methodSymbol.Parameters.Select(symbol => symbol.ToDisplayString()))}})
                         {
                              return Task.Run(() => {{methodSymbol.Name}}({{string.Join(", ", methodSymbol.Parameters.Select(symbol => symbol.Name))}}));
                         }
                         """);
        }
        else
        {
            cb.AddFunction($$"""
                             public Task<{{methodSymbol.ReturnType}}> {{methodSymbol.Name}}Async({{string.Join(", ", methodSymbol.Parameters.Select(symbol => symbol.ToDisplayString()))}})
                             {
                                  return Task.Run(() => {{methodSymbol.Name}}({{string.Join(", ", methodSymbol.Parameters.Select(symbol => symbol.Name))}}));
                             }
                             """);
        }
    }
}

[UsedImplicitly]
public class ToStringExtend : IExtendable
{
    public void ExtendClass(ClassBuilder cb, INamedTypeSymbol classSymbol, SemanticModel semanticModel,
        ImmutableArray<AdditionalText> additionalFile)
    {
        cb.AddFunction($$"""
                       public override string ToString()
                       {
                           return "{{classSymbol.Name}}";
                       }
                       """);
    }
}

[UsedImplicitly]
public class SetFieldExtend(string? fieldName, string? returnValue) : IExtendable
{
    public void ExtendMethod(ClassBuilder cb, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        var field = fieldName ?? methodSymbol.Name.RemoveStart("set", StringComparison.OrdinalIgnoreCase);
        var type = methodSymbol.Parameters.Single().Type.ToDisplayString();
        var retType = methodSymbol.ReturnType;
        
        cb.AddField($"{type} {field};");
        cb.AddFunction($$"""
                         {{methodSymbol.DeclaredAccessibility.ToString().ToLower()}} partial {{(retType.SpecialType == SpecialType.System_Void ? "void" : retType.ToDisplayString())}} {{methodSymbol.Name}}({{type}} {{methodSymbol.Parameters.Single().Name}})
                         {
                            {{field}} = {{methodSymbol.Parameters.Single().Name}};
                            {{(retType.SpecialType == SpecialType.System_Void ? "" : GetReturn(methodSymbol))}}
                         }
                         """);
    }

    private string GetReturn(IMethodSymbol methodSymbol)
    {
        if (returnValue == null)
            return "return null;";
        return $"return {returnValue ?? throw new InvalidOperationException($"Function {methodSymbol.ToDisplayString()} do not return null, set parameter returnValue of SetFieldAttribute")};";
    }
}


[UsedImplicitly]
public class GetFieldExtend(string? fieldName) : IExtendable
{
    public void ExtendMethod(ClassBuilder cb, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        var field = fieldName ?? methodSymbol.Name.RemoveStart("get", StringComparison.OrdinalIgnoreCase);
        var type = methodSymbol.ReturnType;
        
        cb.AddFunction($$"""
                         {{methodSymbol.DeclaredAccessibility.ToString().ToLower()}} partial {{type}} {{methodSymbol.Name}}()
                         {
                            return {{field}};
                         }
                         """);
    }
}


[UsedImplicitly]
public class MockFromJsonExtend(ITypeSymbol interfaceToImplement) : IExtendable
{
    public void ExtendClass(ClassBuilder cb, INamedTypeSymbol classSymbol, SemanticModel semanticModel,
        ImmutableArray<AdditionalText> additionalFile)
    {
        var str = additionalFile.Single(text => text.Path.EndsWith(".json")).GetText()?.ToString();
        
        if (str == null)
            return;
        var json = JsonNode.Parse(str);

        foreach (var member in interfaceToImplement.GetMembers())
        {
            if (member is IPropertySymbol p)
            {
                cb.AddProperty(WriteProperty(p));
            }
            else if (member is IMethodSymbol m 
                     && m.MethodKind != MethodKind.PropertyGet
                     && m.MethodKind != MethodKind.PropertySet)
            {
                cb.AddFunction(WriteMethode(m));
            }
        }
    }
    
    private string WriteProperty(IPropertySymbol propertySymbol)
    {
        throw new NotImplementedException();
    }
    
    private string WriteMethode(IMethodSymbol methodSymbol)
    {
        throw new NotImplementedException();
    }
}


