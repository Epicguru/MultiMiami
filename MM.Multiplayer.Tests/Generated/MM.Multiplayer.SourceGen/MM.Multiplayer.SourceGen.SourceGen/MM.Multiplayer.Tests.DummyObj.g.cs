// Class: MM.Multiplayer.Tests.DummyObj
// Client RPC Count: 0
// Sync Var Count: 2
// Debug output:

namespace MM.Multiplayer.Tests;

internal partial class DummyObj
{
    // Class body...
    private struct GeneratedVars_DummyObj
    {
        public int SomeNumber_TickLastSynched = -10_000;

        public System.Int32 SomeNumber_LastSyncedValue = default;

        public GeneratedVars_DummyObj() { }
    }

    private GeneratedVars_DummyObj generatedVars_DummyObj = new GeneratedVars_DummyObj();

    protected override void HandleSyncVarRead(Lidgren.Network.NetIncomingMessage msg, uint id)
    {
        switch (id)
        {
            case 1:
                // SomeNumber
                SomeNumber = msg.ReadInt32();
                generatedVars_DummyObj.SomeNumber_TickLastSynched = 0;
                break;

            default:
                base.HandleSyncVarRead(msg, id);
                break;
        }
    }

    public override void WriteSyncVars(Lidgren.Network.NetOutgoingMessage msg)
    {
        int defaultInterval = DefaultSyncVarInterval;

        // SomeNumber [1]
        if (MM.Multiplayer.Net.Tick - generatedVars_DummyObj.SomeNumber_TickLastSynched >= defaultInterval && generatedVars_DummyObj.SomeNumber_LastSyncedValue != SomeNumber)
        {
            msg.WriteVariableUInt32(1);
            msg.Write(SomeNumber);
            generatedVars_DummyObj.SomeNumber_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_DummyObj.SomeNumber_LastSyncedValue = SomeNumber;
        }

        base.WriteSyncVars(msg);
    }
    public override void WriteInitialNetData(Lidgren.Network.NetOutgoingMessage msg)
    {
        msg.Write(InitOnly);

        base.WriteInitialNetData(msg);
    }

    public override void ReadInitialNetData(Lidgren.Network.NetIncomingMessage msg)
    {
        InitOnly = msg.ReadSingle();

        base.ReadInitialNetData(msg);
    }
}
