using Microsoft.CodeAnalysis;

namespace MM.Multiplayer.SourceGen;

public struct ClientRPC
{
    public ITypeSymbol Class;
    public IMethodSymbol Method;
    public AttributeData Attribute;
}
