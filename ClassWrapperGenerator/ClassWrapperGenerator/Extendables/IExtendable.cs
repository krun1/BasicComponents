using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ClassWrapperGenerator.Extendables;

public interface IExtendable
{
    void ExtendClass(ClassBuilder cb, INamedTypeSymbol classSymbol, SemanticModel semanticModel, ImmutableArray<AdditionalText> additionalFile)
    {
        throw new System.NotImplementedException();
    }

    void ExtendMethod(ClassBuilder cb, IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        throw new System.NotImplementedException();
    }

    void ExtendProperty(ClassBuilder cb, IPropertySymbol propertySymbol, SemanticModel semanticModel)
    {
        throw new System.NotImplementedException();
    }

    void ExtendParameter(ClassBuilder sb, IParameterSymbol parameterSymbol, SemanticModel semanticModel)
    {
        throw new System.NotImplementedException();
    }

    void ExtendField(ClassBuilder sb, IFieldSymbol fieldSymbol, SemanticModel semanticModel)
    {
        throw new System.NotImplementedException();
    }
}
