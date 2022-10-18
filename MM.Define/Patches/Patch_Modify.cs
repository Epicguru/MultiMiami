using System.Xml;

namespace MM.Define.Patches;

public class Patch_Modify : DefPatch
{
    public Operation Operation = Operation.ModifyOrAdd;
    public XmlNode Value;

    public override PatchOutcome TryExecute(XmlNode document)
    {
        var outcome = new PatchOutcome();
        var nodes = document.SelectNodes(Path);

        switch (Operation)
        {
            case Operation.Add:

                if (Value?.FirstElement() == null)
                    return outcome.Fail("Cannot have null <Value> when <Operation> is Add.");

                var toAdd = document.OwnerDocument.ImportNode(Value.FirstElement(), true);

                foreach (XmlNode node in nodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    var existing = node[toAdd.Name];
                    if (existing != null)
                        continue;

                    node.AppendChild(toAdd.CloneNode(true));
                    outcome.ModificationCount++;
                }
                break;

            case Operation.Modify:
                break;

            case Operation.ModifyOrAdd:
                break;

            case Operation.Remove:
                break;

            default:
                throw new ArgumentOutOfRangeException(Operation.ToString());
        }

        outcome.WasSuccess = outcome.ModificationCount > 0;
        return outcome;
    }
}

public enum Operation
{
    Add,
    Modify,
    ModifyOrAdd,
    Remove
}