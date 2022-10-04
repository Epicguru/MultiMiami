using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MM.Multiplayer.SourceGen;

internal class RPCGen
{
    private readonly ClassUnit unit;
    private readonly SourceWriter str;
    private readonly GeneratorExecutionContext ctx;

    public RPCGen(in GeneratorExecutionContext ctx, SourceWriter str, ClassUnit unit)
    {
        this.ctx = ctx;
        this.unit = unit;
        this.str = str;
    }

    public void Generate()
    {
        GenerateFields();
        GenerateServerRPCs(unit.RPCs.Where(r => !r.IsClientRPC));
        GenerateServerHandler(unit.RPCs.Where(r => !r.IsClientRPC));
        GeneratePatchMethod();
    }

    private void GenerateServerRPCs(IEnumerable<RPC> rpcs)
    {
        int i = 0;
        foreach (var rpc in rpcs)
        {
            // Check for invalid mods.
            foreach (var param in rpc.MethodSyntax.ParameterList.Parameters)
            {
                if (param.Modifiers.Any(m => m.ValueText != "in"))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(Errors.RPCParamHasMod, param.GetLocation(), param.GetText(), rpc.Method.Name));
                    return;
                }
            }

            // Open prefix method.
            str.WriteLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");

            // Method signature.
            str.Write("private static bool ").Write(rpc.PrefixMethodName).Write("(");
            str.Write(rpc.Class.FullName()).Write(" __instance");
            foreach (var param in rpc.Method.Parameters)
            {
                str.Write(", ");
                str.Write(param.Type.FullName()).Write(' ').Write(param.Name);
            }
            str.WriteLine(')');
            str.WriteLine('{');
            str.Indent();

            str.Write("if (disablePatch_").Write(rpc.PrefixMethodName).WriteLine(')');
            str.Indent();
            str.WriteLine("return true;");
            str.Outdent();
            str.WriteLine();

            // Disallow server rpc if client is not running.
            str.WriteLine("#if DEBUG");
            str.WriteLine("if (!MM.Multiplayer.GameClient.IsRunning)");
            str.Indent();
            str.WriteLine("throw new Exception(\"Should not invoke server RPC on a standalone server.\");");
            str.Outdent();
            str.WriteLine("#endif");
            str.WriteLine();

            // Call base method if invoked on the server.
            str.WriteLine("if (MM.Multiplayer.GameServer.IsRunning)");
            str.Indent();
            str.WriteLine("return true;");
            str.WriteLine();
            str.Outdent();

            if (i == 255)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Errors.TooManyRPC, rpc.Class.Locations.First(), rpc.Class.FullName()));
                return;
            }

            // Create message
            str.WriteLine("var msg = MM.Multiplayer.GameClient.Instance.CreateMessage(64);");
            str.WriteLine("msg.Write((byte)4);"); // Base msg type.
            str.WriteLine("msg.Write(__instance.NetID);"); // Target object
            str.Write("msg.Write((byte)").Write((unit.StartServerRPC + i++).ToString()).WriteLine(");"); // Target method
            str.WriteLine();

            // Write parameters.
            foreach (var param in rpc.Method.Parameters)
            {
                str.Write("msg.Write(").Write(param.Name).WriteLine(");");
            }

            // Send message.
            str.WriteLine();
            str.WriteLine("MM.Multiplayer.GameClient.Instance.SendMessage(msg, Lidgren.Network.NetDeliveryMethod.ReliableSequenced, 0);");
            str.WriteLine("return false;");

            str.Outdent();
            str.WriteLine('}');
            str.WriteLine();
        }
    }

    private void GenerateServerHandler(IEnumerable<RPC> rpcs)
    {
        if (!rpcs.Any())
            return;

        str.WriteLine("public override void HandleServerRPC(Lidgren.Network.NetIncomingMessage msg, System.Byte id)");
        str.WriteLine('{');
        str.Indent();

        str.WriteLine("switch (id)");
        str.WriteLine('{');
        str.Indent();

        int i = 0;
        foreach (var rpc in rpcs)
        {
            int id = unit.StartServerRPC + i++;
            str.Write("case ").Write(id.ToString()).WriteLine(':');
            str.Indent();
            str.Write("disablePatch_").Write(rpc.PrefixMethodName).WriteLine(" = true;");

            str.Write(rpc.Method.Name).Write('(');

            bool first = true;
            foreach (var param in rpc.Method.Parameters)
            {
                if (!first)
                    str.Write(", ");

                str.Write("msg.Read").Write(param.Type.Name).Write("()");

                first = false;
            }
            str.WriteLine(");");

            str.Write("disablePatch_").Write(rpc.PrefixMethodName).WriteLine(" = false;");
            str.WriteLine("break;");
            str.Outdent();
            str.WriteLine();
        }

        // Base case.
        str.WriteLine("default:");
        str.Indent();
        str.WriteLine("base.HandleServerRPC(msg, id);");
        str.WriteLine("break;");
        str.Outdent();

        // Leave switch.
        str.Outdent();
        str.WriteLine('}');

        // Leave method.
        str.Outdent();
        str.WriteLine('}').WriteLine();
    }

    private void GeneratePatchMethod()
    {
        str.WriteLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        str.Write("private static void Generated_OnNetRegistered_").Write(unit.Name).WriteLine("()");
        str.WriteLine('{');

        if (unit.RPCs.Count == 0)
        {
            str.WriteLine('}');
            return;
        }

        str.Indent();

        str.WriteLine("System.Reflection.MethodBase orig;");
        str.WriteLine("HarmonyLib.HarmonyMethod pre;");
        str.WriteLine();

        SourceWriter TypeOfSelf()
        {
            return str.Write("typeof(").Write(unit.Class.FullName()).Write(')');
        }

        void WriteParams(RPC rpc)
        {
            foreach (var param in rpc.Method.Parameters)
            {
                str.Write("typeof(").Write(param.Type.FullName()).WriteLine("),");
            }
        }

        // Server RPCs.
        foreach (var rpc in unit.RPCs)
        {
            if (rpc.IsClientRPC)
                continue;

            str.Comment(rpc.Method.Name);

            // orig = typeof(DummyObj).GetMethod("RPCName", BindingFlags.Instance | BindingFlags.XXX, new Type[]
            str.Write("orig = ");
            TypeOfSelf().Write(".GetMethod(\"").Write(rpc.Method.Name);
            str.Write("\", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.");
            str.Write(rpc.MethodSyntax.IsPublic() ? "Public" : "NonPublic").WriteLine(", new Type[]");
            str.WriteLine('{');

            str.Indent();
            WriteParams(rpc);
            str.Outdent();
            str.WriteLine("});");

            // pre = new HarmonyMethod(typeof(DummyObj), "prefixname", new Type[]
            str.Write("pre = new HarmonyLib.HarmonyMethod(");
            TypeOfSelf();
            str.Write(", \"").Write(rpc.PrefixMethodName).WriteLine("\", new Type[]");
            str.WriteLine('{');
            str.Indent();
            TypeOfSelf().WriteLine(',');
            WriteParams(rpc);
            str.Outdent();
            str.WriteLine("});");

            str.WriteLine("MM.Multiplayer.Net.Harmony.Patch(orig, prefix: pre);");
        }

        str.Outdent();
        str.WriteLine('}');
    }

    private void GenerateFields()
    {
        str.WriteLine();
        foreach (var rpc in unit.RPCs)
        {
            str.WriteLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            str.Write("private static bool disablePatch_").Write(rpc.PrefixMethodName).WriteLine(';');
        }
        str.WriteLine();
    }
}
