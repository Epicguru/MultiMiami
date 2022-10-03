using JetBrains.Annotations;

namespace MM.Multiplayer;

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public class SyncVarAttribute : Attribute
{
    public readonly int SyncInterval;
    public readonly string CallbackMethodName;
    public readonly bool InitOnly;

    public SyncVarAttribute(int syncInterval = -1, string callbackMethodName = null, bool initOnly = false)
    {
        SyncInterval = syncInterval;
        CallbackMethodName = callbackMethodName;
        InitOnly = initOnly;
    }
}
