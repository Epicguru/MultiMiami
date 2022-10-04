using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace MM.Multiplayer.SourceGen;

public class RPC
{
    public readonly string UUID = Guid.NewGuid().ToString().Replace('-', '_');
    public string PrefixMethodName => preCached ??= $"GeneratedPrefix_{Method.Name}_{UUID}";
    public ITypeSymbol Class;
    public IMethodSymbol Method;
    public MethodDeclarationSyntax MethodSyntax;
    public AttributeData Attribute;
    public bool IsClientRPC;

    private string preCached;
}
