using System.Xml;
using JetBrains.Annotations;

namespace MM.Define.Patches;

[UsedImplicitly(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
public abstract class DefPatch
{
    public string Path;

    public abstract PatchOutcome TryExecute(XmlNode document);
}