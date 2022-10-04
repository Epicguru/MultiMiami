using Microsoft.CodeAnalysis;

namespace MM.Multiplayer.SourceGen;

internal static class SyncVarGen
{
    private static ClassUnit unit;
    private static SourceWriter str;
    private static string structName;
    private static string structFieldName;

    public static void GenFor(in GeneratorExecutionContext ctx, SourceWriter str, ClassUnit unit)
    {
        if (unit.SyncVars == null)
            return;

        if (!Diagnose(ctx, unit))
            return;

        SyncVarGen.str = str;
        SyncVarGen.unit = unit;

        // Regular vars.
        if (unit.HasAnyRegularVars)
        {
            // Structure.
            OpenStruct();
            WriteStruct();
            CloseStruct();

            // Fields:
            MakeFields();

            // Read method:
            WriteHandleReadMethod();

            // Write method:
            WriteHandleWriteMethod();
        }
        
        // Init-only vars.
        if (unit.HasAnyInitOnlyVars)
        {
            WriteWriteInitialDataMethod();
            WriteReadInitialDataMethod();
        }
    }

    private static bool Diagnose(in GeneratorExecutionContext ctx, ClassUnit unit)
    {
        if (unit.SyncVars.Count == 0)
            return false;

        foreach (var sv in unit.SyncVars)
        {
            if (sv.Field.IsStatic)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Errors.StaticSyncVar, sv.Field.Locations[0], sv.Field.Name));
                return false;
            }

            if (sv.Field.IsReadOnly)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Errors.SyncVarReadOnly, sv.Field.Locations[0], sv.Field.Name));
                return false;
            }
        }

        return true;
    }

    private static void OpenStruct()
    {
        structName = $"GeneratedVars_{unit.Name}";
        structFieldName = $"generatedVars_{unit.Name}";

        str.Write("private struct ").WriteLine(structName);
        str.WriteLine('{');
        str.Indent();
    }

    private static void WriteStruct()
    {
        // Time since synced:
        foreach (var syncVar in unit.SyncVars)
        {
            if (syncVar.IsInitOnly)
                continue;

            str.Write("public int ");
            str.Write(syncVar.TickLastWrittenName);
            str.WriteLine(" = -10_000;");
        }

        str.WriteLine();

        // Last synced value
        foreach (var syncVar in unit.SyncVars)
        {
            if (syncVar.IsInitOnly)
                continue;

            str.Write("public ");
            str.Write(syncVar.Field.Type.FullName()).Write(' ');
            str.Write(syncVar.LastValueName);
            str.WriteLine(" = default;");
        }

        // Default constructor.
        str.WriteLine();
        str.Write("public ").Write(structName).WriteLine("() { }");
    }

    private static void CloseStruct()
    {
        str.Outdent();
        str.WriteLine('}').WriteLine();
    }

    private static void MakeFields()
    {
        // Struct field.
        str.Write("private ");
        str.Write(structName);
        str.Write(' ');
        str.Write(structFieldName);
        str.Write(" = new ");
        str.Write(structName);
        str.WriteLine("();");
    }

    private static void WriteHandleReadMethod()
    {
        str.WriteLine();
        str.WriteLine(@"protected override void HandleSyncVarRead(Lidgren.Network.NetIncomingMessage msg, uint id)");
        str.WriteLine('{');
        str.Indent();
        str.WriteLine("switch (id)");
        str.WriteLine('{');
        str.Indent();

        uint j = 0;
        for (int i = 0; i < unit.SyncVars.Count; i++)
        {
            var sVar = unit.SyncVars[i];

            if (sVar.IsInitOnly)
                continue;

            str.Write("case ").Write((j + unit.SyncVarBaseID).ToString()).WriteLine(':');
            str.Indent();
            str.Comment(sVar.Field.Name);
            WriteRead(sVar);
            str.WriteLine("break;");
            str.Outdent();
            str.WriteLine();

            j++;
        }

        // Default clause.
        str.WriteLine("default:");
        str.Indent();
        str.WriteLine("base.HandleSyncVarRead(msg, id);");
        str.WriteLine("break;");
        str.Outdent();

        // Close switch.
        str.Outdent();
        str.WriteLine('}');
        str.Outdent();

        // Close method.
        str.WriteLine('}');
    }

    private static void WriteRead(in SyncVar sVar)
    {
        string updateMethod = sVar.Attribute.TryGetValue("CallbackMethodName")?.Value?.ToString();


        if (updateMethod == null)
        {
            // varName = msg.ReadVarType();
            str.Write(sVar.Field.Name).Write(" = ").Write("msg.Read");
            str.Write(sVar.Field.Type.Name);
            str.WriteLine("();");
        }
        else
        {
            // Call custom update method.
            str.Write("var newValue = msg.Read").Write(sVar.Field.Type.Name);
            str.WriteLine("();");

        }
        

        // generatedStructFieldName.varTimeSinceLastSync = 0;
        str.Write(structFieldName).Write('.').Write(sVar.TickLastWrittenName);
        str.WriteLine(" = 0;");

        if (updateMethod != null)
        {
            str.Write(updateMethod).WriteLine("(newValue);");
        }
    }

    private static void WriteHandleWriteMethod()
    {
        str.WriteLine();
        str.WriteLine(@$"public override void WriteSyncVars(Lidgren.Network.NetOutgoingMessage msg)");
        str.WriteLine('{');
        str.Indent();

        str.WriteLine("int defaultInterval = DefaultSyncVarInterval;");
        str.WriteLine();

        uint j = 0;
        for (int i = 0; i < unit.SyncVars.Count; i++)
        {
            var sVar = unit.SyncVars[i];
            if (sVar.IsInitOnly)
                continue;

            uint id = j + unit.SyncVarBaseID;

            str.Comment($"{sVar.Field.Name} [{id}]");
            WriteWrite(sVar, id);
            j++;
        }

        // Base call.
        str.WriteLine("base.WriteSyncVars(msg);");

        // Close method.
        str.Outdent();
        str.WriteLine('}');
    }

    private static void WriteWrite(in SyncVar sVar, uint id)
    {
        // Read attribute info.
        int? intervalInt = sVar.Attribute.TryGetValue("SyncInterval")?.Value as int?;
        string interval = intervalInt is null or -1 ? "defaultInterval" : intervalInt.ToString();

        // Write if statement.
        str.Write("if (MM.Multiplayer.Net.Tick - ");
        str.Write(structFieldName).Write('.').Write(sVar.TickLastWrittenName);
        str.Write(" >= ").Write(interval).Write(" && ");
        str.Write(structFieldName).Write('.').Write(sVar.LastValueName);
        str.Write(" != ");
        str.Write(sVar.Field.Name);
        str.WriteLine(')');
        str.WriteLine('{');

        str.Indent();

        // Write ID and data to message.
        str.Write("msg.WriteVariableUInt32(").Write(id.ToString());
        str.WriteLine(");");
        str.Write("msg.Write(").Write(sVar.Field.Name).WriteLine(");");

        // Update last sent time.
        str.Write(structFieldName).Write('.').Write(sVar.TickLastWrittenName);
        str.WriteLine(" = MM.Multiplayer.Net.Tick;");

        // Update last sent value.
        str.Write(structFieldName).Write('.').Write(sVar.LastValueName);
        str.Write(" = ").Write(sVar.Field.Name).WriteLine(';');
        str.Outdent();
        str.WriteLine('}').WriteLine();
    }

    private static void WriteWriteInitialDataMethod()
    {
        str.WriteLine(@"public override void WriteInitialNetData(Lidgren.Network.NetOutgoingMessage msg)");
        str.WriteLine('{');
        str.Indent();

        foreach (var sv in unit.SyncVars)
        {
            if (!sv.IsInitOnly)
                continue;

            str.Write("msg.Write(");
            str.Write(sv.Field.Name);
            str.WriteLine(");");
        }

        str.WriteLine();
        str.WriteLine("base.WriteInitialNetData(msg);");
        str.Outdent();
        str.WriteLine('}');
        str.WriteLine();
    }

    private static void WriteReadInitialDataMethod()
    {
        str.WriteLine(@"public override void ReadInitialNetData(Lidgren.Network.NetIncomingMessage msg)");
        str.WriteLine('{');
        str.Indent();

        foreach (var sv in unit.SyncVars)
        {
            if (!sv.IsInitOnly)
                continue;

            str.Write(sv.Field.Name);
            str.Write(" = ");
            str.Write("msg.Read");
            str.Write(sv.Field.Type.Name);
            str.WriteLine("();");
        }

        str.WriteLine();
        str.WriteLine("base.ReadInitialNetData(msg);");
        str.Outdent();
        str.WriteLine('}');
    }
}
