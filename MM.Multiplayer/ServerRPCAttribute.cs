namespace MM.Multiplayer;

/// <summary>
/// Designates a method that is invoked on a client but run on the server.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ServerRPCAttribute : Attribute
{

}
