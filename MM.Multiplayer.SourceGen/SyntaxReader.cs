using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MM.Multiplayer.SourceGen
{
    public sealed class SyntaxReader : ISyntaxReceiver
    {
        public const string CLIENT_RPC_NAME = "MM.Multiplayer.Remote.ClientRPC";

        public readonly Dictionary<ClassDeclarationSyntax, ClassUnit> Classes = new Dictionary<ClassDeclarationSyntax, ClassUnit>(128);

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax method)
                VisitMethod(method);
        }

        private void VisitMethod(MethodDeclarationSyntax method)
        {
            var clientRpcAttr = method.TryGetAttribute(CLIENT_RPC_NAME);

            if (clientRpcAttr != null)
            {
                VisitClientRPC(method, clientRpcAttr);
                return;
            }
        }

        private void VisitClientRPC(MethodDeclarationSyntax method, AttributeSyntax attr)
        {
            var raw = new ClientRPC
            {
                Attribute = attr,
                Method = method,
                Class = method.GetDeclaringClass()
            };

            GetUnit(raw.Class).ClientRPCs.Add(raw);
        }

        private ClassUnit GetUnit(ClassDeclarationSyntax @class)
        {
            if (Classes.TryGetValue(@class, out var found))
                return found;

            found = new ClassUnit(@class);
            Classes.Add(@class, found);
            return found;
        }
    }
}
