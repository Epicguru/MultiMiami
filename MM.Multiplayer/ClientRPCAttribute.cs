namespace MM.Multiplayer;

/// <summary>
/// Designates a method that is invoked on the server but run on every client.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ClientRPCAttribute : Attribute
{

}
