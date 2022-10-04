using JetBrains.Annotations;

namespace MM.Multiplayer;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class NetObject : SyncVarOwner
{
    public bool IsSpawned => NetID != 0;

    public ushort NetID;

    public override string ToString() => $"[{GetType().Name}:{NetID}]";
}
