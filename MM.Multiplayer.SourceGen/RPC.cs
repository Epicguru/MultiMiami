using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace MM.Multiplayer.SourceGen;

public class RPC
{
    public string PrefixMethodName => preCached ??= $"GeneratedPrefix_{Method.ContainingType.Name.Replace('+', '_').Replace('.', '_')}_{Method.Name}";
    public ITypeSymbol Class;
    public IMethodSymbol Method;
    public MethodDeclarationSyntax MethodSyntax;
    public AttributeData Attribute;
    public bool IsClientRPC;

    private string preCached;
}
