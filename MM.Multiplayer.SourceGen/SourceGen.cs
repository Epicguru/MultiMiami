using System.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace MM.Multiplayer.SourceGen;

[Generator]
public class SourceGen : ISourceGenerator
{
    private readonly SourceWriter str = new SourceWriter(1024 * 4);

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a factory that can create our custom syntax receiver
        context.RegisterForSyntaxNotifications(() => new SyntaxReader());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // the generator infrastructure will create a receiver and populate it
        // we can retrieve the populated instance via the context
        var reader = (SyntaxReader)context.SyntaxContextReceiver;

        //if (!Debugger.IsAttached)
        //    Debugger.Launch();

        foreach (var unit in reader.Classes.Values)
        {
            unit.Parent = reader.Classes.Values.FirstOrDefault(u =>
                u.Class.Equals(unit.Class.BaseType, SymbolEqualityComparer.Default));
        }

        foreach (var pair in reader.Classes)
        {
            GenerateFor(context, pair.Value);
        }
    }

    private void GenerateFor(in GeneratorExecutionContext ctx, ClassUnit unit)
    {
        str.Clear();

        if (!unit.Syntax.IsPartial())
        {
            ctx.ReportDiagnostic(Diagnostic.Create(Errors.ClassNotPartial, unit.Syntax.GetLocation(), unit.FullName));
            return;
        }

        // Diagnostics.
        str.Comment("Class", unit.FullName);
        str.Comment("Client RPC Count", unit.ClientRPCs.Count.ToString());
        str.Comment("Sync Var Count", unit.SyncVars.Count.ToString());
        str.Comment("Debug output:");
        foreach (var debug in unit.DebugOutput)
        {
            str.Comment(debug);
        }

        str.WriteLine();

        WriteNamespace(unit);

        OpenClass(unit);

        // Class contents here...
        SyncVarGen.GenFor(ctx, str, unit);

        CloseClass(unit);

        ctx.AddSource($"{unit.FullName}.g.cs", str.ToString());
    }

    private void WriteNamespace(ClassUnit unit)
    {
        var ns = unit.Class.TryGetNamespace();
        if (ns == null)
            return;

        str.Write("namespace ").Write(ns).WriteLine(';').WriteLine();
    }

    private void OpenClass(ClassUnit unit)
    {
        foreach (var mod in unit.Syntax.Modifiers)
        {
            str.Write(mod.ValueText).Write(' ');
        }
        str.Write("class ");
        str.WriteLine(unit.Name);
        str.WriteLine('{');
        str.Indent();
        str.Comment("Class body...");
    }

    private void CloseClass(ClassUnit _)
    {
        str.Outdent();
        str.WriteLine('}');
    }
}