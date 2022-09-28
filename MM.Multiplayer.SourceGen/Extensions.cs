using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MM.Multiplayer.SourceGen
{
    public static class Extensions
    {
        public static string TryGetNamespace(this SyntaxNode syntax)
        {
            // If we don't have a namespace at all we'll return an empty string
            // This accounts for the "default namespace" case
            string nameSpace = null;

            // Get the containing syntax node for the type declaration
            // (could be a nested type, for example)
            SyntaxNode potentialNamespaceParent = syntax.Parent;

            // Keep moving "out" of nested classes etc until we get to a namespace
            // or until we run out of parents
            while (potentialNamespaceParent != null &&
                   !(potentialNamespaceParent is NamespaceDeclarationSyntax)
                   && !(potentialNamespaceParent is FileScopedNamespaceDeclarationSyntax))
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            // Build up the final namespace by looping until we no longer have a namespace declaration
            if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
            {
                // We have a namespace. Use that as the type
                nameSpace = namespaceParent.Name.ToString();

                // Keep moving "out" of the namespace declarations until we 
                // run out of nested namespace declarations
                while (true)
                {
                    if (!(namespaceParent.Parent is NamespaceDeclarationSyntax parent))
                    {
                        break;
                    }

                    // Add the outer namespace as a prefix to the final namespace
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            // return the final namespace
            return nameSpace;
        }

        public static string FullName(this AttributeSyntax attribute)
        {
            string root = attribute.TryGetNamespace();
            return root != null ? $"{root}.{attribute.Name}" : attribute.Name.ToString();
        }

        public static string FullName(this ClassDeclarationSyntax @class)
        {
            string root = @class.TryGetNamespace();
            return root != null ? $"{root}.{@class.Identifier.ValueText}" : @class.Identifier.ValueText;
        }

        public static AttributeSyntax TryGetAttribute(this MethodDeclarationSyntax method, string fullName)
        {
            foreach (var attrList in method.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.FullName() == fullName)
                        return attr;
                }
            }

            return null;
        }

        public static ClassDeclarationSyntax GetDeclaringClass(this MethodDeclarationSyntax method)
        {
            if (method.Parent is ClassDeclarationSyntax @class)
                return @class;

            throw new System.Exception("Tried to get declaring class for a method that is not part of a class (local method, lambda, or part of struct etc.)");
        }

        public static void Comment(this StringBuilder builder, string str)
        {
            builder.Append("// ").AppendLine(str);
        }

        public static void Comment(this StringBuilder builder, string key, string value)
        {
            builder.Append("// ").Append(key).Append(": ").AppendLine(value);
        }
    }
}
