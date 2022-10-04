using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MM.Multiplayer.SourceGen;

public class ClassUnit
{
    public ClassUnit Parent { get; set; }
    public uint SyncVarBaseID
    {
        get
        {
            if (Parent == null)
                return 1;

            Parent.nonInitSyncVarCount ??= (uint)Parent.SyncVars.Count(sv => !sv.IsInitOnly);

            return (uint)(Parent.SyncVarBaseID + Parent.nonInitSyncVarCount);
        }
    }
    public bool HasAnyInitOnlyVars => SyncVars.Any(sv => sv.IsInitOnly);
    public bool HasAnyRegularVars => SyncVars.Any(sv => !sv.IsInitOnly);


    public readonly string Name;
    public readonly string FullName;
    public readonly ITypeSymbol Class;
    public readonly List<ClientRPC> ClientRPCs = new List<ClientRPC>();
    public readonly List<SyncVar> SyncVars = new List<SyncVar>();
    public readonly List<string> DebugOutput = new List<string>();
    public readonly ClassDeclarationSyntax Syntax;

    private uint? nonInitSyncVarCount;

    public ClassUnit(ITypeSymbol @class)
    {
        Name = @class.Name;
        Class = @class;
        FullName = @class.FullName();
        Syntax = @class.DeclaringSyntaxReferences.Select(d => d.GetSyntax() as ClassDeclarationSyntax).First();
    }
}