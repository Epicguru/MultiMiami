using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MM.Multiplayer.SourceGen
{
    public class ClassUnit
    {
        public readonly ClassDeclarationSyntax Class;
        public readonly List<ClientRPC> ClientRPCs = new List<ClientRPC>();

        public ClassUnit(ClassDeclarationSyntax @class)
        {
            Class = @class;
        }
    }
}
