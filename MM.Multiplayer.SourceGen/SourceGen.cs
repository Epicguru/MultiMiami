using Microsoft.CodeAnalysis;
using System.Text;

namespace MM.Multiplayer.SourceGen
{
    [Generator]
    public class SourceGen : ISourceGenerator
    {
        private readonly StringBuilder str = new StringBuilder(1024 * 4);

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new SyntaxReader());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            var reader = (SyntaxReader)context.SyntaxReceiver;

            foreach (var pair in reader.Classes)
            {
                GenerateFor(context, pair.Value);
            }
        }

        private void GenerateFor(in GeneratorExecutionContext ctx, ClassUnit unit)
        {
            str.Clear();

            // Diagnostics.
            str.Comment("Class", unit.Class.FullName());
            str.Comment("Client RPC Count", unit.ClientRPCs.Count.ToString());

            ctx.ReportDiagnostic(Diagnostic.Create("fucky wucky", "Generator", "You done fucked up", DiagnosticSeverity
                .Error, DiagnosticSeverity.Error, true, 0));
        }
    }
}