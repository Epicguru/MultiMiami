using Microsoft.CodeAnalysis;

namespace MM.Multiplayer.SourceGen;

public struct SyncVar
{
    public ITypeSymbol DeclaringClass;
    public IFieldSymbol Field;
    public AttributeData Attribute;
    public string TicksSinceLastName;
    public string LastValueName;
    public bool IsInitOnly;
}
