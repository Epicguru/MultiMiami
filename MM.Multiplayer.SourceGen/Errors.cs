using Microsoft.CodeAnalysis;

namespace MM.Multiplayer.SourceGen;

internal static class Errors
{
    public static readonly DiagnosticDescriptor ClassNotPartial = new DiagnosticDescriptor
    (
        "MM001",
        "SyncVarOwner-derived classes should be partial",
        "Class {0} should be marked as partial",
        "Multiplayer.SyncVars",
        DiagnosticSeverity.Error, true
    );

    public static readonly DiagnosticDescriptor SyncVarReadOnly = new DiagnosticDescriptor
    (
        "MM002",
        "SyncVars can not be read-only",
        "SyncVar Field {0} should not be marked as readonly",
        "Multiplayer.SyncVars",
        DiagnosticSeverity.Error, true
    );

    public static readonly DiagnosticDescriptor StaticSyncVar = new DiagnosticDescriptor
    (
        "MM003",
        "SyncVars can not be static fields",
        "SyncVar Field {0} should not be marked as a SyncVar, or should be made an instance field",
        "Multiplayer.SyncVars",
        DiagnosticSeverity.Error, true
    );

    public static readonly DiagnosticDescriptor RPCParamHasMod = new DiagnosticDescriptor
    (
        "MM004",
        "RPC method parameters cannot have modifiers (except in)",
        "Parameter '{0}' on RPC method {1} has an invalid modifier. The only allowed modifier is 'in'.",
        "Multiplayer.RPCs",
        DiagnosticSeverity.Error, true
    );

    public static readonly DiagnosticDescriptor TooManyRPC = new DiagnosticDescriptor
    (
        "MM005",
        "Too many RPCs",
        "There are too many (>255) client/server RPCs in the class {0} (includes inherited RPCs)'",
        "Multiplayer.RPCs",
        DiagnosticSeverity.Error, true
    );

    public static readonly DiagnosticDescriptor ClassNested = new DiagnosticDescriptor
    (
        "MM006",
        "SyncVarOwner-derived classes can not be nested classes",
        "Class {0} should not be a nested (inner) class",
        "Multiplayer.SyncVars",
        DiagnosticSeverity.Error, true
    );
}
