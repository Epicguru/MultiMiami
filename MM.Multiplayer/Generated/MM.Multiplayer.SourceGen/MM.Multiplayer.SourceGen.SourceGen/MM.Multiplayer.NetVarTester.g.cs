﻿// Class: MM.Multiplayer.NetVarTester
// Client RPC Count: 1
// Sync Var Count: 3
// Debug output:

namespace MM.Multiplayer;

public partial class NetVarTester
{
    // Class body...
    private struct GeneratedVars_NetVarTester
    {
        public int IntNetVar_TickLastSynched = -10_000;
        public int FloatNetVar_TickLastSynched = -10_000;
        public int MySyncString_TickLastSynched = -10_000;

        public System.Int32 IntNetVar_LastSyncedValue = default;
        public System.Single FloatNetVar_LastSyncedValue = default;
        public System.String MySyncString_LastSyncedValue = default;

        public GeneratedVars_NetVarTester() { }
    }

    private GeneratedVars_NetVarTester generatedVars_NetVarTester = new GeneratedVars_NetVarTester();

    protected override void HandleSyncVarRead(Lidgren.Network.NetIncomingMessage msg, uint id)
    {
        switch (id)
        {
            case 1:
                // IntNetVar
                IntNetVar = msg.ReadInt32();
                generatedVars_NetVarTester.IntNetVar_TickLastSynched = 0;
                break;

            case 2:
                // FloatNetVar
                var newValue = msg.ReadSingle();
                generatedVars_NetVarTester.FloatNetVar_TickLastSynched = 0;
                FloatChanged(newValue);
                break;

            case 3:
                // MySyncString
                MySyncString = msg.ReadString();
                generatedVars_NetVarTester.MySyncString_TickLastSynched = 0;
                break;

            default:
                base.HandleSyncVarRead(msg, id);
                break;
        }
    }

    public override void WriteSyncVars(Lidgren.Network.NetOutgoingMessage msg)
    {
        int defaultInterval = DefaultSyncVarInterval;

        // IntNetVar [1]
        if (MM.Multiplayer.Net.Tick - generatedVars_NetVarTester.IntNetVar_TickLastSynched >= defaultInterval && generatedVars_NetVarTester.IntNetVar_LastSyncedValue != IntNetVar)
        {
            msg.WriteVariableUInt32(1);
            msg.Write(IntNetVar);
            generatedVars_NetVarTester.IntNetVar_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_NetVarTester.IntNetVar_LastSyncedValue = IntNetVar;
        }

        // FloatNetVar [2]
        if (MM.Multiplayer.Net.Tick - generatedVars_NetVarTester.FloatNetVar_TickLastSynched >= 30 && generatedVars_NetVarTester.FloatNetVar_LastSyncedValue != FloatNetVar)
        {
            msg.WriteVariableUInt32(2);
            msg.Write(FloatNetVar);
            generatedVars_NetVarTester.FloatNetVar_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_NetVarTester.FloatNetVar_LastSyncedValue = FloatNetVar;
        }

        // MySyncString [3]
        if (MM.Multiplayer.Net.Tick - generatedVars_NetVarTester.MySyncString_TickLastSynched >= 5 && generatedVars_NetVarTester.MySyncString_LastSyncedValue != MySyncString)
        {
            msg.WriteVariableUInt32(3);
            msg.Write(MySyncString);
            generatedVars_NetVarTester.MySyncString_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_NetVarTester.MySyncString_LastSyncedValue = MySyncString;
        }

        base.WriteSyncVars(msg);
    }
}
