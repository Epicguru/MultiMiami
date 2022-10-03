namespace MM.Multiplayer.Shared;

[AttributeUsage(AttributeTargets.Field)]
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
