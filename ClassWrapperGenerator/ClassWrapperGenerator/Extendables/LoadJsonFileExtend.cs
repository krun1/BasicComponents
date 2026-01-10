using System;
using System.Linq;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace ClassWrapperGenerator.Extendables;

[UsedImplicitly]
public class LoadJsonFileExtend(object[] ignoredField) : IExtendable
{
    private readonly string[]? _ignoredField = ignoredField.Cast<string>().ToArray();
    
    private string JsonToObject(string n, string varName, ITypeSymbol type)
    {
        var sb = new CodeBuilder();
        
        sb.Append($"var {varName} = new {type.ToDisplayString()}()");
        sb.AppendLine();
        
        foreach (var ps in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (ps.IsReadOnly || (_ignoredField?.Contains(ps.Name) ?? false))
                continue;
            sb.AppendLine($"{JsonToValue($"""{n}["{ps.Name}"]""", $"{varName}.{ps.Name}", ps.Type)}");
        }
        return sb.ToString();
    }

    private string JsonToValue(string value, string varName, ITypeSymbol type)
    {
        var typeSymbol = (type as INamedTypeSymbol)?.TypeArguments;
        
        switch (type.SpecialType)
        {
            case SpecialType.None when type.NullableAnnotation == NullableAnnotation.Annotated 
                                       && typeSymbol?.FirstOrDefault()?.IsValueType == true:
                value = $"{varName} = {value}.GetValue<{((INamedTypeSymbol)type).TypeArguments.FirstOrDefault()?.FullName()}?>();";
                break;
            case SpecialType.None when type.TypeKind == TypeKind.Enum:
            case SpecialType.System_Enum:
            case SpecialType.System_Boolean:
            case SpecialType.System_Char:
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Decimal:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_String:
                value = $"{varName} = {value}.GetValue<{type.FullName()}>();";
                break;
            case SpecialType.None when type.FullName() == typeof(Guid).FullName:
                value = $"{varName} = System.Guid.Parse({value}.GetValue<string>());";
                break;
            case SpecialType.None when type.FullName() == typeof(Uri).FullName:
                value = $"{varName} = new System.Uri({value}.GetValue<string>());";
                break;
            case SpecialType.None when type.TypeKind == TypeKind.Array:
            case SpecialType.System_Array:
                value = $$"""
                          {{varName}} = {{value}}.AsArray().Select(n =>
                          {
                              {{JsonToValue("n", "i", ((IArrayTypeSymbol)type).ElementType)}})
                              return i;
                          }.ToArray();
                          """;
                break;
            case SpecialType.None when type.Name.Contains("List"):
            case SpecialType.System_Collections_IEnumerable:
            case SpecialType.System_Collections_Generic_IEnumerable_T:
            case SpecialType.System_Collections_Generic_IList_T:
            case SpecialType.System_Collections_Generic_ICollection_T:
            case SpecialType.System_Collections_Generic_IReadOnlyList_T:
            case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                value = $$"""
                          {{varName}} = {{value}}.AsArray().Select(n => 
                          {
                              {{JsonToValue("n", "i", ((INamedTypeSymbol)type).TypeArguments.Single())}}
                              return i;
                          }).ToList();
                          """;
                break;
            case SpecialType.System_Collections_IEnumerator:
                break;
            case SpecialType.System_Collections_Generic_IEnumerator_T:
                break;
            case SpecialType.System_DateTime:
                value = $"{varName} = {value}.GetValue<{type.FullName()}>();";
                break;
            default:
                var newVarValue = varName.Replace(".", "_");
                value = JsonToObject(value, newVarValue, type);
                value += $"{varName} = {newVarValue}";
                break;
        }

        return value;
    }

    public void ExtendMethod(ClassBuilder cb, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        var type = (INamedTypeSymbol)methodSymbol.ReturnType;
        var accessibility = methodSymbol.DeclaredAccessibility.ToString().ToLower();
        var param = methodSymbol.Parameters.Single();
        var name = param.Name;

        cb.AddUsing("System.Text.Json.Nodes");
        cb.AddUsing("System.Linq");
        var sb = new CodeBuilder(1);

        if (param.Type.FullName() == typeof(string).FullName)
            sb.AppendLine($"var o = JsonNode.Parse({name});");
        else if (param.Type.FullName() ==  typeof(JsonNode).FullName)
            sb.AppendLine($"var o = {name};");
        else
            throw new InvalidOperationException($"param {name} of type {param.Type.FullName()} is not supported");
        sb.AppendLine();
        sb.Append($"{JsonToObject("o", "res", type)}");

        var func = $$"""
                     {{accessibility}} partial {{type.ToDisplayString()}} {{methodSymbol.Name}}({{param.Type.ToDisplayString()}} {{name}})
                     {
                     {{sb}};
                        return res;
                     }
                     """;

        cb.AddFunction(func);
    }
}