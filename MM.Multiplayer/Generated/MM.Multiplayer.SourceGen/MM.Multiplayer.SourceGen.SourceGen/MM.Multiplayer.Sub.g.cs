// Class: MM.Multiplayer.Sub
// Client RPC Count: 0
// Sync Var Count: 6
// Debug output:

namespace MM.Multiplayer;

public partial class Sub
{
    // Class body...
    private struct GeneratedVars_Sub
    {
        public int MyByte_TickLastSynched = -10_000;
        public int HitPoints_TickLastSynched = -10_000;
        public int works_TickLastSynched = -10_000;
        public int doesNotWork_TickLastSynched = -10_000;
        public int staticDoesNotWork_TickLastSynched = -10_000;

        public System.Byte MyByte_LastSyncedValue = default;
        public System.UInt16 HitPoints_LastSyncedValue = default;
        public System.UInt32 works_LastSyncedValue = default;
        public System.Int64 doesNotWork_LastSyncedValue = default;
        public System.Double staticDoesNotWork_LastSyncedValue = default;

        public GeneratedVars_Sub() { }
    }

    private GeneratedVars_Sub generatedVars_Sub = new GeneratedVars_Sub();

    protected override void HandleSyncVarRead(Lidgren.Network.NetIncomingMessage msg, uint id)
    {
        switch (id)
        {
            case 4:
                // MyByte
                var newValue = msg.ReadByte();
                generatedVars_Sub.MyByte_TickLastSynched = 0;
                ByteCallback(newValue);
                break;

            case 5:
                // HitPoints
                HitPoints = msg.ReadUInt16();
                generatedVars_Sub.HitPoints_TickLastSynched = 0;
                break;

            case 6:
                // works
                works = msg.ReadUInt32();
                generatedVars_Sub.works_TickLastSynched = 0;
                break;

            case 7:
                // doesNotWork
                doesNotWork = msg.ReadInt64();
                generatedVars_Sub.doesNotWork_TickLastSynched = 0;
                break;

            case 8:
                // staticDoesNotWork
                staticDoesNotWork = msg.ReadDouble();
                generatedVars_Sub.staticDoesNotWork_TickLastSynched = 0;
                break;

            default:
                base.HandleSyncVarRead(msg, id);
                break;
        }
    }

    public override void WriteSyncVars(Lidgren.Network.NetOutgoingMessage msg)
    {
        int defaultInterval = DefaultSyncVarInterval;

        // MyByte [4]
        if (MM.Multiplayer.Net.Tick - generatedVars_Sub.MyByte_TickLastSynched >= defaultInterval && generatedVars_Sub.MyByte_LastSyncedValue != MyByte)
        {
            msg.WriteVariableUInt32(4);
            msg.Write(MyByte);
            generatedVars_Sub.MyByte_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_Sub.MyByte_LastSyncedValue = MyByte;
        }

        // HitPoints [5]
        if (MM.Multiplayer.Net.Tick - generatedVars_Sub.HitPoints_TickLastSynched >= defaultInterval && generatedVars_Sub.HitPoints_LastSyncedValue != HitPoints)
        {
            msg.WriteVariableUInt32(5);
            msg.Write(HitPoints);
            generatedVars_Sub.HitPoints_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_Sub.HitPoints_LastSyncedValue = HitPoints;
        }

        // works [6]
        if (MM.Multiplayer.Net.Tick - generatedVars_Sub.works_TickLastSynched >= defaultInterval && generatedVars_Sub.works_LastSyncedValue != works)
        {
            msg.WriteVariableUInt32(6);
            msg.Write(works);
            generatedVars_Sub.works_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_Sub.works_LastSyncedValue = works;
        }

        // doesNotWork [7]
        if (MM.Multiplayer.Net.Tick - generatedVars_Sub.doesNotWork_TickLastSynched >= defaultInterval && generatedVars_Sub.doesNotWork_LastSyncedValue != doesNotWork)
        {
            msg.WriteVariableUInt32(7);
            msg.Write(doesNotWork);
            generatedVars_Sub.doesNotWork_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_Sub.doesNotWork_LastSyncedValue = doesNotWork;
        }

        // staticDoesNotWork [8]
        if (MM.Multiplayer.Net.Tick - generatedVars_Sub.staticDoesNotWork_TickLastSynched >= defaultInterval && generatedVars_Sub.staticDoesNotWork_LastSyncedValue != staticDoesNotWork)
        {
            msg.WriteVariableUInt32(8);
            msg.Write(staticDoesNotWork);
            generatedVars_Sub.staticDoesNotWork_TickLastSynched = MM.Multiplayer.Net.Tick;
            generatedVars_Sub.staticDoesNotWork_LastSyncedValue = staticDoesNotWork;
        }

        base.WriteSyncVars(msg);
    }
    public override void WriteInitialNetData(Lidgren.Network.NetOutgoingMessage msg)
    {
        msg.Write(SomeDouble);

        base.WriteInitialNetData(msg);
    }

    public override void ReadInitialNetData(Lidgren.Network.NetIncomingMessage msg)
    {
        SomeDouble = msg.ReadDouble();

        base.ReadInitialNetData(msg);
    }
}
