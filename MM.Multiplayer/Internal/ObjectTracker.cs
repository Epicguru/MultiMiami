using Lidgren.Network;
using MM.Logging;
using System.Diagnostics;

namespace MM.Multiplayer.Internal;

public partial class ObjectTracker
{
    public const int MAX_TRACKED_OBJECTS = 1024 * 8;
    public const int MAX_NET_OBJECT_TYPES = 512;

    public int TrackedObjectCount { get; private set; }
    public bool ShouldSendSyncVars { get; set; }
    public Action<NetOutgoingMessage> SendMessage, SendExceptHost, Recycle;
    public Func<NetOutgoingMessage> CreateMessage;

    private readonly NetObject[] objects = new NetObject[MAX_TRACKED_OBJECTS];
    private ushort maxNetID = 1;

    public bool Spawn(NetObject obj)
    {
        if (obj == null)
        {
            Log.Error("Tried to spawn null object");
            return false;
        }

        var t = obj.GetType();

        if (obj.IsSpawned)
        {
            Log.Error($"Tried to spawn object {obj} that is already spawned!");
            return false;
        }

        var data = GetTypeData(t);
        if (!data.IsValid)
        {
            Log.Error($"Cannot spawn net object of type '{t.FullName}' because that type has not been registered");
            return false;
        }

        ushort newID = AllocateNewNetID();
        if (newID == 0)
        {
            Log.Error($"Cannot spawn more net objects, already reached the max {MAX_TRACKED_OBJECTS} objects!");
            return false;
        }

        // Register to storage.
        obj.NetID = newID;
        objects[newID] = obj;
        TrackedObjectCount++;

        var msg = CreateMessage();

        // Type is spawn.
        msg.Write((byte) 1);

        // Type ID and net ID.
        msg.Write(data.ID);
        msg.Write(newID);

        // All object initial data, such as sync vars.
        obj.WriteInitialNetData(msg);

        SendExceptHost(msg);

        return true;
    }

    public void OnReceiveSpawnMessage(NetIncomingMessage msg)
    {
        ushort typeID = msg.ReadUInt16();
        ushort netID = msg.ReadUInt16();

        Log.Trace($"Type: {typeID}");
        Log.Trace($"NetID: {netID}");

        var obj = objects[netID];
        if (obj != null)
        {
            Log.Error($"Received spawn message for object that already exists ({netID}) ..."); 
            return;
        }

        var instance = CreateInstance<NetObject>(typeID);
        if (instance == null)
        {
            Log.Error($"Failed to spawn net net object of type {typeID}: this type has not been registered.");
            return;
        }

        Log.Trace("Made instance.");


        // Register to storage.
        instance.NetID = netID;
        objects[netID] = instance;
        TrackedObjectCount++;

        // Read initial data such as sync vars.
        instance.ReadInitialNetData(msg);

        Log.Trace("Read initial data.");
    }

    private ushort AllocateNewNetID()
    {
        return maxNetID++;
    }

    public IEnumerable<NetObject> GetAllTrackedObjects()
    {
        for (int i = 1; i <= TrackedObjectCount; i++)
        {
            yield return objects[i];
        }
    }

    public void OnReceiveSyncVarMessage(NetIncomingMessage msg)
    {
        ushort objID;
        while ((objID = msg.ReadUInt16()) != 0)
        {
            var obj = objects[objID];
            if (obj == null)
                continue;

            obj.ReadSyncVars(msg);
        }
    }

    public void Tick()
    {
        if (ShouldSendSyncVars)
        {
            // Write all.
            var msg = CreateMessage();
            msg.Write((byte) 3);

            // 2 bytes for ID + 1 byte for end-of-sync flag.
            const int CONSTANT_SIZE_BITS = (2 + 1) * 8;

            foreach (var obj in GetAllTrackedObjects())
            {
                int start = msg.LengthBits;
                msg.Write(obj.NetID);
                obj.WriteSyncVars(msg);

                if (msg.LengthBits <= start + CONSTANT_SIZE_BITS)
                {
                    Debug.Assert(msg.LengthBits == start + CONSTANT_SIZE_BITS);
                    msg.LengthBits -= CONSTANT_SIZE_BITS;
                }
            }

            if (msg.LengthBits > 8)
            {
                msg.Write((ushort) 0); // End flag.
                SendExceptHost(msg);
            }
            else
            {
                //Recycle(msg);
            }
        }
    }

    public NetObject TryGetObject(ushort id) => objects[id];
}
