using JetBrains.Annotations;
using Lidgren.Network;

namespace MM.Multiplayer;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class NetObject : SyncVarOwner
{
    public bool IsSpawned => NetID != 0;

    public ushort NetID;

    public void Spawn()
    {
        if (IsSpawned)
            return;

        GameServer.Instance.Spawn(this);
    }

    public virtual void HandleServerRPC(NetIncomingMessage msg, byte id)
    {
        throw new Exception($"Failed to find Server RPC handler for ID {id}");
    }

    public override string ToString() => $"[{GetType().Name}:{NetID}]";
}
