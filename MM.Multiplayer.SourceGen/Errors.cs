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
}
