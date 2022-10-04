using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MM.Multiplayer.SourceGen;

public class ClassUnit
{
    public ClassUnit Parent { get; set; }
    public byte SyncVarBaseID
    {
        get
        {
            if (Parent == null)
                return 1;

            Parent.nonInitSyncVarCount ??= (byte)Parent.SyncVars.Count(sv => !sv.IsInitOnly);

            return (byte)(Parent.SyncVarBaseID + Parent.nonInitSyncVarCount);
        }
    }
    public bool HasAnyInitOnlyVars => SyncVars.Any(sv => sv.IsInitOnly);
    public bool HasAnyRegularVars => SyncVars.Any(sv => !sv.IsInitOnly);
    public int StartClientRPC { get; private set; }
    public int StartServerRPC { get; private set; }

    public readonly string Name;
    public readonly string FullName;
    public readonly ITypeSymbol Class;
    public readonly List<RPC> RPCs = new List<RPC>();
    public readonly List<SyncVar> SyncVars = new List<SyncVar>();
    public readonly List<string> DebugOutput = new List<string>();
    public readonly ClassDeclarationSyntax Syntax;

    private bool donePreGenerate;
    private uint? nonInitSyncVarCount;
    private int maxClientRPC, maxServerRPC;

    public ClassUnit(ITypeSymbol @class)
    {
        Name = @class.Name;
        Class = @class;
        FullName = @class.FullName();
        Syntax = @class.DeclaringSyntaxReferences.Select(d => d.GetSyntax() as ClassDeclarationSyntax).First();
    }

    public void PreGenerate(in GeneratorExecutionContext ctx)
    {
        if (donePreGenerate)
            return;
        donePreGenerate = true;

        Parent?.PreGenerate(ctx);

        int clientCount = RPCs.Count(r => r.IsClientRPC);
        int serverCount = RPCs.Count - clientCount;

        if (Parent == null)
        {
            maxClientRPC = clientCount;
            maxServerRPC = serverCount;
        }
        else
        {
            maxClientRPC = Parent.maxClientRPC + clientCount;
            maxServerRPC = Parent.maxServerRPC + serverCount;
            StartClientRPC = Parent.maxClientRPC;
            StartServerRPC = Parent.maxServerRPC;
        }
    }
}