using System.Text;
using System.Xml;

namespace MM.Define;

public static class Extensions
{
    private static readonly StringBuilder str = new StringBuilder();

    public static string GetAttributeValue(this XmlNode node, string attrName, string defaultValue = null)
    {
        var attr = node.Attributes[attrName];
        if (attr == null)
            return defaultValue;
        return attr.Value;
    }

    public static bool GetAttributeAsBool(this XmlNode node, string attrName, bool defaultValue = false)
    {
        string value = node.GetAttributeValue(attrName);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public static Type StripNullable(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

    public static void SetAttribute(this XmlNode node, string name, string value)
    {
        var found = node.Attributes[name];
        if (found != null)
        {
            found.Value = value;
            return;
        }

        var created = node.OwnerDocument.CreateAttribute(name);
        created.Value = value;
        node.Attributes.Append(created);
    }

    public static XmlNode FirstElement(this XmlNode node)
    {
        foreach (XmlNode child in node)
            if (child.NodeType == XmlNodeType.Element)
                return child;
        return null;
    }

    public static string GetFullPath(this XmlNode node)
    {
        str.Clear();

        string NodeToString(XmlNode n)
        {
            if (n.ParentNode == null || n.ParentNode.ChildNodes.Count == 1)
                return n.Name;

            int self = -1;
            int i = 0;
            foreach (XmlNode sibling in n.ParentNode.ChildNodes)
            {
                if (sibling == n)
                {
                    self = i++;
                    continue;
                }

                if (sibling.NodeType == n.NodeType && sibling.Name == n.Name)
                {
                    i++;
                }
            }

            if (i > 1)
                return $"{n.Name}[{self}]";
            return n.Name;
        }

        var current = node;
        while (current != null)
        {
            if (current != node)
                str.Insert(0, '/');
            str.Insert(0, NodeToString(current));
            current = current.ParentNode;
        }

        return str.ToString();
    }
}