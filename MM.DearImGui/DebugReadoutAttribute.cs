using JetBrains.Annotations;

namespace MM.DearImGui;

[MeansImplicitUse(ImplicitUseKindFlags.Access)]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DebugReadoutAttribute : Attribute
{
    public string Name { get; set; }
    public string Category { get; set; }
}