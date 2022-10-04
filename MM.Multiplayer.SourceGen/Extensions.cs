using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MM.Multiplayer.SourceGen;

public static class Extensions
{
    public static string FullName(this ITypeSymbol namedType)
    {
        string output = namedType.Name;
        var ns = namedType.ContainingNamespace;
        while (ns is { IsGlobalNamespace: false })
        {
            output = $"{ns.Name}.{output}";
            ns = ns.ContainingNamespace;
        }
        return output;
    }

    public static string TryGetNamespace(this ITypeSymbol namedType)
    {
        string output = null;
        var ns = namedType.ContainingNamespace;
        while (ns is { IsGlobalNamespace: false })
        {
            output = output == null ? ns.Name : $"{ns.Name}.{output}";
            ns = ns.ContainingNamespace;
        }
        return output;
    }

    public static bool DoesInheritFrom(this ITypeSymbol @class, string parent)
    {
        var current = @class.BaseType;
        while (current != null)
        {
            if (current.FullName() == parent)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    public static AttributeData TryGetAttribute(this ISymbol method, string fullName)
    {
        foreach (var attr in method.GetAttributes())
        {
            if (attr.AttributeClass.FullName() == fullName)
                return attr;
        }
        return null;
    }

    public static TypedConstant? TryGetValue(this AttributeData attr, string name)
    {
        // Search named arguments...
        foreach (var pair in attr.NamedArguments)
        {
            if (pair.Key == name)
                return pair.Value;
        }

        // Search normal constructor.
        int i = 0;
        foreach (var param in attr.AttributeConstructor.Parameters)
        {
            if (param.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                if (attr.ConstructorArguments.Length > i)
                    return attr.ConstructorArguments[i];
                
                return null;
            }

            i++;
        }

        return null;
    }

    public static bool IsPartial(this ClassDeclarationSyntax @class)
    {
        foreach (var mod in @class.Modifiers)
        {
            if (mod.ValueText.Equals("partial"))
                return true;
        }

        return false;
    }

    public static bool IsPublic(this MethodDeclarationSyntax method)
    {
        foreach (var mod in method.Modifiers)
        {
            if (mod.ValueText.Equals("public"))
                return true;
        }

        return false;
    }
}
