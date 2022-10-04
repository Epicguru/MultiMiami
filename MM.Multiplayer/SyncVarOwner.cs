using Lidgren.Network;
using MM.Logging;

namespace MM.Multiplayer;

public abstract class SyncVarOwner
{
    /// <summary>
    /// The default interval, measured in ticks,
    /// between sending updates to net vars when said net vars have changed.
    /// Can be override on a per-variable basis by using <see cref="SyncVarAttribute.SyncInterval"/>.
    /// </summary>
    public virtual int DefaultSyncVarInterval => 6;

    /// <summary>
    /// Reads all sync vars from the message.
    /// </summary>
    public void ReadSyncVars(NetIncomingMessage msg)
    {
        byte id;
        while ((id = msg.ReadByte()) != 0)
        {
            HandleSyncVarRead(msg, id);
        }
    }

    /// <summary>
    /// Called when reading sync vars from a net message.
    /// Called once for each incoming sync var.
    /// The <paramref name="id"/> is the internal sync var ID.
    /// </summary>
    protected virtual void HandleSyncVarRead(NetIncomingMessage msg, byte id)
    {
        Log.Error($"Unhandled SyncVar with ID {id}");
    }

    /// <summary>
    /// Writes all dirty sync vars to the net message.
    /// </summary>
    public virtual void WriteSyncVars(NetOutgoingMessage msg)
    {
        // Indicates end of sync vars.
        msg.Write((byte)0);
    }

    /// <summary>
    /// Called on the server when this object is first spawned or when a new client connects.
    /// writes all required data to the message.
    /// By default, this writes the sync vars.
    /// </summary>
    public virtual void WriteInitialNetData(NetOutgoingMessage msg)
    {
        WriteSyncVars(msg);
    }

    /// <summary>
    /// Called on the connected clients when this object is first spawned. Reads all required data.
    /// By default, this reads the sync vars.
    /// </summary>
    public virtual void ReadInitialNetData(NetIncomingMessage msg)
    {
        ReadSyncVars(msg);
    }
}
