using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MM.Multiplayer.SourceGen;

public sealed class SyntaxReader : ISyntaxContextReceiver
{
    public const string CLIENT_RPC_NAME = "MM.Multiplayer.ClientRPCAttribute";
    public const string SYNC_VAR_NAME = "MM.Multiplayer.SyncVarAttribute";
    public const string SYNCVAR_OWNER_NAME = "MM.Multiplayer.SyncVarOwner";

    public readonly Dictionary<string, ClassUnit> Classes = new Dictionary<string, ClassUnit>(128);

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        switch (context.Node)
        {
            case MethodDeclarationSyntax method:
                VisitMethod(context.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol);
                break;
            case FieldDeclarationSyntax field:
                foreach (var v in field.Declaration.Variables)
                    VisitField(context.SemanticModel.GetDeclaredSymbol(v) as IFieldSymbol);
                break;
        }
    }

    private void VisitMethod(IMethodSymbol method)
    {
        if (method == null)
            return;

        var declaringClass = method.ReceiverType;
        if (declaringClass == null)
            return;

        if (!declaringClass.DoesInheritFrom(SYNCVAR_OWNER_NAME))
            return;

        var clientRpc = method.TryGetAttribute(CLIENT_RPC_NAME);
        if (clientRpc != null)
            VisitClientRPC(method, declaringClass, clientRpc);

        return;
    }

    private void VisitField(IFieldSymbol field)
    {
        if (field == null)
            return;

        var declaringClass = field.ContainingType;
        if (declaringClass == null)
            return;

        if (!declaringClass.DoesInheritFrom(SYNCVAR_OWNER_NAME))
            return;

        var attr = field.TryGetAttribute(SYNC_VAR_NAME);
        if (attr != null)
            VisitSyncVar(declaringClass, field, attr);
    }

    private void VisitSyncVar(INamedTypeSymbol declaringClass, IFieldSymbol field, AttributeData attr)
    {
        var raw = new SyncVar
        {
            DeclaringClass = declaringClass,
            Field = field,
            Attribute = attr,
            TicksSinceLastName = $"{field.Name}_TicksSinceSync",
            LastValueName = $"{field.Name}_LastSyncedValue",
            IsInitOnly = (bool?)attr.TryGetValue("InitOnly")?.Value ?? false
        };
        GetUnit(declaringClass).SyncVars.Add(raw);
    }

    private void VisitClientRPC(IMethodSymbol method, ITypeSymbol declaringClass, AttributeData attr)
    {
        var raw = new ClientRPC
        {
            Attribute = attr,
            Method = method,
            Class = declaringClass
        };

        GetUnit(declaringClass).ClientRPCs.Add(raw);
    }

    private ClassUnit GetUnit(ITypeSymbol type, bool create = true)
    {
        if (Classes.TryGetValue(type.FullName(), out var found))
            return found;
        if (!create)
            return null;

        found = new ClassUnit(type);
        Classes.Add(found.FullName, found);
        return found;
    }
}