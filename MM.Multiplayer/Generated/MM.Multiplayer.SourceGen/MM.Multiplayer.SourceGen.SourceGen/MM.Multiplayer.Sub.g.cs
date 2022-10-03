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
        public int MyByte_TicksSinceSync = 1024;
        public int HitPoints_TicksSinceSync = 1024;
        public int works_TicksSinceSync = 1024;
        public int doesNotWork_TicksSinceSync = 1024;
        public int staticDoesNotWork_TicksSinceSync = 1024;

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
            case 3:
                // MyByte
                var newValue = msg.ReadByte();
                generatedVars_Sub.MyByte_TicksSinceSync = 0;
                ByteCallback(newValue);
                break;

            case 4:
                // HitPoints
                HitPoints = msg.ReadUInt16();
                generatedVars_Sub.HitPoints_TicksSinceSync = 0;
                break;

            case 5:
                // works
                works = msg.ReadUInt32();
                generatedVars_Sub.works_TicksSinceSync = 0;
                break;

            case 6:
                // doesNotWork
                doesNotWork = msg.ReadInt64();
                generatedVars_Sub.doesNotWork_TicksSinceSync = 0;
                break;

            case 7:
                // staticDoesNotWork
                staticDoesNotWork = msg.ReadDouble();
                generatedVars_Sub.staticDoesNotWork_TicksSinceSync = 0;
                break;

            default:
                base.HandleSyncVarRead(msg, id);
                break;
        }
    }

    public override void WriteSyncVars(Lidgren.Network.NetOutgoingMessage msg)
    {
        int defaultInterval = DefaultSyncVarInterval;

        // MyByte [3]
        if (generatedVars_Sub.MyByte_TicksSinceSync >= defaultInterval && generatedVars_Sub.MyByte_LastSyncedValue != MyByte)
        {
            msg.WriteVariableUInt32(3);
            msg.Write(MyByte);
            generatedVars_Sub.MyByte_TicksSinceSync = 0;
            generatedVars_Sub.MyByte_LastSyncedValue = MyByte;
        }

        // HitPoints [4]
        if (generatedVars_Sub.HitPoints_TicksSinceSync >= defaultInterval && generatedVars_Sub.HitPoints_LastSyncedValue != HitPoints)
        {
            msg.WriteVariableUInt32(4);
            msg.Write(HitPoints);
            generatedVars_Sub.HitPoints_TicksSinceSync = 0;
            generatedVars_Sub.HitPoints_LastSyncedValue = HitPoints;
        }

        // works [5]
        if (generatedVars_Sub.works_TicksSinceSync >= defaultInterval && generatedVars_Sub.works_LastSyncedValue != works)
        {
            msg.WriteVariableUInt32(5);
            msg.Write(works);
            generatedVars_Sub.works_TicksSinceSync = 0;
            generatedVars_Sub.works_LastSyncedValue = works;
        }

        // doesNotWork [6]
        if (generatedVars_Sub.doesNotWork_TicksSinceSync >= defaultInterval && generatedVars_Sub.doesNotWork_LastSyncedValue != doesNotWork)
        {
            msg.WriteVariableUInt32(6);
            msg.Write(doesNotWork);
            generatedVars_Sub.doesNotWork_TicksSinceSync = 0;
            generatedVars_Sub.doesNotWork_LastSyncedValue = doesNotWork;
        }

        // staticDoesNotWork [7]
        if (generatedVars_Sub.staticDoesNotWork_TicksSinceSync >= defaultInterval && generatedVars_Sub.staticDoesNotWork_LastSyncedValue != staticDoesNotWork)
        {
            msg.WriteVariableUInt32(7);
            msg.Write(staticDoesNotWork);
            generatedVars_Sub.staticDoesNotWork_TicksSinceSync = 0;
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
