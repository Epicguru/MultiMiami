using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MM.Multiplayer.SourceGen
{
    public struct ClientRPC
    {
        public ClassDeclarationSyntax Class;
        public MethodDeclarationSyntax Method;
        public AttributeSyntax Attribute;
    }
}
