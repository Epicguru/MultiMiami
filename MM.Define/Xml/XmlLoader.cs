using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MM.Define.Patches;
using MM.Define.Xml.Internal;
using MM.Define.Xml.Parsers;

namespace MM.Define.Xml;

public class XmlLoader : IDisposable
{
    /*
     * Attributes:
     *
     * >> C# <<
     * [XmlIgnore] -- This member is always ignored when loading/saving XML, regardless of config.
     * [XmlInclude] -- This member is always included when loading/saving XML, regardless of config.
     * [Include] -- This member is always included when loading/saving Ceras, regardless of config.
     * [Exclude] -- This member is always excluded when loading/saving Ceras, regardless of config.
     * [Alias("MyName")] -- This member can be assigned from an XML node node called "MyName" as well as the normal member name.
     *
     * >> XML <<
     * Abstract="true/false" -- Specifies that this def node is abstract or not. Defaults to false if not specified. Only valid on def root node.
     * Parent="ParentName" -- Specifies that the parent of this def is the def named "ParentName".
     * Type="TypeName" -- Specifies the C# type of this node.
     * ElementType="TypeName" -- Specifies the C# type of element types. Only valid for Lists and Dictionaries. The specified type should be assignable to the base type of this list's elements.
     * KeyType="TypeName" -- Specifies the C# type of key types in a dictionary. Only valid for Dictionaries. The specified type should be assignable to the base type of this dictionary's keys.
     * Inherit="true/false" -- If false, the base value from a parent node is ignored, and is instead entirely replaced with the new child value. Not valid on def root node.
     * Null="true/false" -- If true, the node is treated as a null value and the value is forcibly written to the owning object.
     * IsList="true/false" -- If true, the node is treated as a list.
     */

    private static readonly HashSet<string> doNotInheritAttributeNames = new HashSet<string>
    {
        "Abstract",
        "Null"
    };

    public bool HasResolvedInheritance { get; private set; }

    public readonly DefLoadConfig Config;

    private readonly List<XmlParser> allParsers = new List<XmlParser>();
    private readonly Dictionary<Type, XmlParser> typeToParser = new Dictionary<Type, XmlParser>();
    private readonly XmlDocument masterDoc = new XmlDocument();
    private readonly Dictionary<Type, MemberStore> fieldMaps = new Dictionary<Type, MemberStore>();
    private readonly HashSet<XmlNode> tempInheritance = new HashSet<XmlNode>();
    private readonly List<XmlNode> tempInheritanceList = new List<XmlNode>();
    private readonly Dictionary<string, IDef> prePopulatedDefs = new Dictionary<string, IDef>();
    private readonly ConfigErrorReporter configReporter = new ConfigErrorReporter();
    private Func<string, IDef> existingDefsFunc;
    private IDef currentDef;

    public XmlLoader(DefLoadConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        // Create master root.
        masterDoc.AppendChild(masterDoc.CreateElement("Defs"));

        AddParser(new DefRefParser());
        AddParser(new EnumParser());
        AddParser(new XmlNodeParser());
        AddParser(new TypeParser());

        AddParser(str => str);
        AddParser(int.Parse);
        AddParser(long.Parse);
        AddParser(float.Parse);
        AddParser(double.Parse);
    }

    private void AddParser<T>(Func<string, T> parseFunc)
    {
        AddParser(new SimpleParser<T>(parseFunc));
    }

    public MemberStore GetMembers(Type type)
    {
        if (fieldMaps.TryGetValue(type, out var found))
            return found;

        var store = new MemberStore(Config, type);
        fieldMaps.Add(type, store);
        return store;
    }

    public XmlParser TryGetParser(Type type)
    {
        if (typeToParser.TryGetValue(type, out var found))
            return found;

        foreach (var parser in allParsers)
        {
            if (!parser.CanHandle(type))
                continue;

            typeToParser.Add(type, parser);
            return parser;
        }

        typeToParser.Add(type, null);
        return null;
    }

    public void AddParser(XmlParser parser)
    {
        if (parser == null)
            throw new ArgumentNullException(nameof(parser));

        if (allParsers.Contains(parser))
            return;

        allParsers.Add(parser);
        typeToParser.Clear(); // Reset cache because type->null is cached for speed reasons.
    }

    private XmlNode GetRootNode(XmlDocument doc)
    {
        foreach (XmlNode child in doc)
        {
            if (child.NodeType == XmlNodeType.Element)
                return child;
        }
        return null;
    }

    public void AppendDocument(XmlDocument document, string source)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var root = GetRootNode(document);
        if (root == null)
        {
            DefDebugger.Warn($"There are no def nodes in the document '{source}':\n{document.InnerXml}");
            return;
        }

        var masterRoot = GetRootNode(masterDoc);

        foreach (XmlNode sub in root)
        {
            if (sub.NodeType != XmlNodeType.Element)
                continue;

            var imported = masterDoc.ImportNode(sub, true);
            masterRoot.AppendChild(imported);

            // Add source attribute for debugging purposes.
            if (source != null)
                imported.SetAttribute("Source", source);
        }
    }

    public void ApplyPatches(XmlDocument document, string source)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var root = GetRootNode(document);
        if (root == null)
        {
            DefDebugger.Warn($"There are no patch nodes in the document '{source}':\n{document.InnerXml}");
            return;
        }

        var masterRoot = GetRootNode(masterDoc);

        foreach (XmlNode patch in root)
        {
            if (patch.NodeType != XmlNodeType.Element)
                continue;

            string typeName = patch.GetAttributeValue("Type");
            if (typeName == null)
            {
                DefDebugger.Error($"Missing Type attribute on a patch in file '{source}'");
                continue;
            }

            var type = TypeResolver.Get(typeName);
            if (type == null)
            {
                DefDebugger.Error($"Failed to resolve patch type called '{typeName}' for a patch in '{source}'");
                continue;
            }

            // Make patch instance.
            DefPatch instance = TryCreateInstance(type, default) as DefPatch;
            if (instance == null)
            {
                DefDebugger.Error($"Failed to create instance of type '{typeName}', or that type does not inherit from  DefPatch.");
                continue;
            }

            // Populate patch instance.
            NodeToClass(new XmlParseContext
            {
                Loader = this,
                CurrentValue = instance,
                DefaultType = typeof(DefPatch),
                TargetType = instance.GetType(),
                Node = patch,
                TextValue = "[[Patch]]"
            });

            try
            {
                var result = instance.TryExecute(masterRoot);
                if (!result.WasSuccess)
                {
                    DefDebugger.Warn($"Patch operation of type '{instance.GetType().Name}' failed. Message: '{result.ErrorMessage}'. {result.ModificationCount} nodes were modified.");
                }
            }
            catch (Exception e)
            {
                DefDebugger.Error($"Patch operation of type '{instance.GetType().Name}' failed with exception.", e);
            }
        }
    }

    public IEnumerable<IDef> MakeDefs() => MakeDefs(null);

    public IEnumerable<IDef> MakeDefs(Func<string, IDef> existingDefs)
    {
        var root = GetRootNode(masterDoc);
        if (root == null)
            yield break;

        currentDef = null;
        prePopulatedDefs.Clear();
        existingDefsFunc = existingDefs;
        var ids = new HashSet<string>();

        if (existingDefs == null)
        {
            foreach (XmlNode sub in root)
            {
                if (sub.NodeType != XmlNodeType.Element)
                    continue;

                if (sub.GetAttributeAsBool("Abstract"))
                    continue;

                var type = GetDefType(sub.Name, sub);
                if (type == null)
                    continue;

                if (TryCreateInstance(type, default) is IDef created)
                    prePopulatedDefs.Add(sub.Name, created);
            }

            existingDefsFunc = str => prePopulatedDefs.TryGetValue(str, out var found) ? found : null;
        }

        foreach (XmlNode sub in root)
        {
            if (sub.NodeType != XmlNodeType.Element)
                continue;

            if (sub.GetAttributeAsBool("Abstract"))
                continue;

            var type = GetDefType(sub.Name, sub, true);
            if (type == null)
                continue;

            if (!ids.Add(sub.Name))
            {
                DefDebugger.Error($"Duplicate def ID: '{sub.Name}'");
                continue;
            }

            var defInstance = existingDefsFunc(sub.Name);
            currentDef = defInstance;
            var parsed = NodeToDef(sub, defInstance);
            if (parsed != null)
                yield return parsed;
        }

        currentDef = null;

        foreach (var parser in allParsers)
        {
            parser.EarlyPostLoad(this);
        }

        prePopulatedDefs.Clear();
        existingDefsFunc = null;
    }

    public IDef TryGetDef(string defID) => existingDefsFunc?.Invoke(defID);

    private Type GetDefType(string id, XmlNode defNode, bool silent = false)
    {
        // Get and validate type.
        string typeName = defNode.GetAttributeValue("Type");
        if (typeName == null)
        {
            if (!silent)
                DefDebugger.Error($"Def '{id}' does not specify a Type using the Type=\"TypeName\" attribute.");
            return null;
        }

        var type = TypeResolver.Get(typeName);
        if (type == null)
        {
            if (!silent)
                DefDebugger.Error($"Def '{id}' is of type '{typeName}', but that type could not be found in any loaded assembly.");
            return null;
        }
        if (type.IsAbstract)
        {
            if (!silent)
                DefDebugger.Error($"Def '{id}' is of type '{typeName}', but that is an abstract type. A concrete subclass must be specified.");
            return null;
        }

        if (!typeof(IDef).IsAssignableFrom(type))
        {
            if (!silent)
                DefDebugger.Error($"Def '{id}' of type '{typeName}' does not implement the IDef interface, so cannot be loaded as a def.");
            return null;
        }

        return type;
    }

    private IDef NodeToDef(XmlNode node, IDef existing)
    {
        // ID is just the node name.
        string id = node.Name;

        Type type = existing?.GetType() ?? GetDefType(id, node);

        // Create def instance.
        var instance = existing ?? TryCreateInstance(type, default) as IDef;
        if (instance == null)
        {
            DefDebugger.Error($"Def '{id}' of type '{node.GetAttributeValue("Type")}' could not be instantiated.");
            return null;
        }
        instance.ID = id;

        // Make context.
        var ctx = new XmlParseContext
        {
            Loader = this,
            CurrentValue = instance,
            DefaultType = type,
            TargetType = type,
            Node = node,
            TextValue = "[[DefNode]]",
            Owner = instance
        };

        var created = NodeToClass(ctx).Value as IDef; // Use NodeToClass instead of NodeToObject to bypass the ref resolver.
        Debug.Assert(created == instance);
        Debug.Assert(created.ID == node.Name);

        if (created is IPostXmlConstruct post)
            post.PostXmlConstruct(ctx);

        created.ID = node.Name;
        return created;
    }

    private Type GetNodeType(XmlNode node, Type defaultType)
    {
        // Check for custom Type attribute.
        string specific = node.GetAttributeValue("Type");
        if (specific != null)
        {
            var found = TypeResolver.Get(specific).StripNullable();
            if (found != null)
                return found;

            DefDebugger.Error($"Failed to find specified type '{specific}'. Falling back to default type '{defaultType}'.");
            return defaultType;
        }

        return defaultType;
    }

    private NodeType GetParseType(in XmlParseContext context)
    {
        var type = context.TargetType;

        if (typeof(IList).IsAssignableFrom(type))
            return NodeType.List;

        if (typeof(IDictionary).IsAssignableFrom(type))
            return NodeType.Dictionary;

        return NodeType.Default;
    }

    public ParseResult NodeToObject(in XmlParseContext context)
    {
        // If the Null attribute is "true", then just return null and make sure it overwrites the existing value.
        if (context.Node.GetAttributeAsBool("Null"))
        {
            // Ignore contents, force write that null back to owner.
            return new ParseResult(null, true);
        }

        var handleMethod = GetParseType(context);
        ParseResult parsed;

        switch (handleMethod)
        {
            case NodeType.Default:

                // Check for simple/raw parser.
                var parser = TryGetParser(context.TargetType);
                if (parser != null)
                {
                    try
                    {
                        parsed = new ParseResult(parser.Parse(context));
                        break;
                    }
                    catch (Exception e)
                    {
                        DefDebugger.Error($"Exception when parsing <{context.Node.Name}> using parser '{parser.GetType().Name}'.", context, e);
                        return default;
                    }
                }

                // Do default node -> class parsing, by writing to each field.
                parsed = NodeToClass(context);
                break;

            case NodeType.List:
                parsed = NodeToList(context);
                break;

            case NodeType.Dictionary:
                parsed = NodeToDictionary(context);
                break;

            default:
                throw new NotImplementedException(handleMethod.ToString());
        }

        // All constructed types are passed through here, so it's a good place to do callbacks...
        // PostXmlConstruct on the created object itself.
        if (parsed.Value is IPostXmlConstruct post)
            post.PostXmlConstruct(context);

        return parsed;
    }

    private ParseResult NodeToList(scoped in XmlParseContext context)
    {
        var listType = context.TargetType;

        Type elementType = listType.IsConstructedGenericType ? listType.GenericTypeArguments[0] : typeof(object);

        string elemOverrideName = context.Node.GetAttributeValue("ElementType");
        if (elemOverrideName != null)
        {
            var type = TypeResolver.Get(elemOverrideName);
            if (type == null)
            {
                DefDebugger.Error($"Failed to find type named '{elemOverrideName}' to use as list element override.", context);
            }
            else if (!elementType.IsAssignableFrom(type))
            {
                DefDebugger.Error($"List element type '{type.FullName}' is not assignable to base list element type '{elementType}'.", context);
            }
            else
            {
                elementType = type;
            }
        }

        var list = (context.CurrentValue ?? TryCreateInstance(listType, context)) as IList;
        if (list == null) // No need to log error, TryCreateInstance already logs.
            return default;

        foreach (XmlNode node in context.Node)
        {
            if (node.NodeType != XmlNodeType.Element)
                continue;

            var type = GetNodeType(node, elementType);
            if (!elementType.IsAssignableFrom(type))
            {
                DefDebugger.Error($"List element type '{type.FullName}' is not assignable to base list element type '{elementType}'.", context);
                continue;
            }

            var ctx = new XmlParseContext
            {
                Loader = this,
                CurrentValue = null,
                DefaultType = elementType,
                TargetType = type,
                ListIndex = list.Count,
                Node = node,
                TextValue = node.InnerText,
                Owner = list
            };

            // Recursive parse call.
            var parsed = NodeToObject(ctx);

            // Add to list!
            if (parsed.ShouldWrite)
                list.Add(parsed.Value);
        }

        return new ParseResult(list);
    }

    private ParseResult NodeToDictionary(scoped in XmlParseContext context)
    {
        var dictType = context.TargetType;

        Type TryGetType(int genericIndex, string attrName, in XmlParseContext context)
        {
            Type elementType = dictType.IsConstructedGenericType && dictType.GenericTypeArguments.Length > genericIndex ? dictType.GenericTypeArguments[genericIndex] : typeof(object);

            string elemOverrideName = context.Node.GetAttributeValue(attrName);
            if (elemOverrideName != null)
            {
                var type = TypeResolver.Get(elemOverrideName);
                if (type == null)
                {
                    DefDebugger.Error($"Failed to find type named '{elemOverrideName}' to use as dictionary key/value override.", context);
                }
                else if (!elementType.IsAssignableFrom(type))
                {
                    DefDebugger.Error($"Dictionary key/value type '{type.FullName}' is not assignable to base dictionary key/value type '{elementType}'.", context);
                }
                else
                {
                    elementType = type;
                }
            }

            return elementType;
        }

        // Resolve key and value types.
        Type keyType = TryGetType(0, "KeyType", context);
        if (keyType == null)
            return default;

        Type valueType = TryGetType(1, "ElementType", context);
        if (valueType == null)
            return default;

        // Get or create dictionary instance.
        var dict = (context.CurrentValue ?? TryCreateInstance(dictType, context)) as IDictionary;
        if (dict == null) // No need to log error, TryCreateInstance already logs.
            return default;

        // Get key parser and validate it.
        var keyParser = TryGetParser(keyType);
        if (keyParser == null)
        {
            DefDebugger.Error($"There is no simple parser for dictionary key type '{keyType}'. A parser for that type should be added using AddParser.", context);
            return new ParseResult(dict);
        }
        if (!keyParser.CanParseNoContext)
        {
            DefDebugger.Error($"The parser '{keyParser.GetType()}' for dictionary key type '{keyType}' does not have the capability to parse with no context, so it that type cannot be used as a dictionary key.", context);
            return new ParseResult(dict);
        }

        foreach (XmlNode node in context.Node)
        {
            if (node.NodeType != XmlNodeType.Element)
                continue;

            // Confirm final value type.
            var localValueType = GetNodeType(node, valueType);
            if (!valueType.IsAssignableFrom(localValueType))
            {
                DefDebugger.Error($"Type '{localValueType}' is not assignable to base dictionary value type '{valueType}'.", context);
                continue;
            }

            // Parse key. Key must not be null.
            var key = keyParser.Parse(new XmlParseContext
            {
                Loader = this,
                TextValue = node.Name,
                DefaultType = keyType,
                TargetType = keyType,
            });
            if (key == null)
            {
                DefDebugger.Error($"Parser '{keyParser.GetType()} returned null when parsing key '{node.Name}' of type '{keyType}' for a dictionary. Dictionary keys cannot be null, so this entry will be discarded.", context);
                continue;
            }

            // Make value context...
            var ctx = new XmlParseContext
            {
                Loader = this,
                CurrentValue = null,
                DefaultType = valueType,
                TargetType = localValueType,
                DictionaryKey = key,
                Owner = dict,
                Node = node,
                TextValue = node.InnerText
            };

            // Recursive parse call.
            var parsed = NodeToObject(ctx);

            // Add to dictionary!
            if (parsed.ShouldWrite)
                dict.Add(key, parsed.Value);
        }

        return new ParseResult(dict);
    }

    private ParseResult NodeToClass(scoped in XmlParseContext context)
    {
        var type = context.TargetType;
        Debug.Assert(type != null);

        // Final type cannot be abstract.
        if (type.IsAbstract)
        {
            DefDebugger.Error($"Cannot create instance of abstract type/interface '{type}'.", context);
            return default;
        }

        // Get current value or create new instance of type.
        var instance = context.CurrentValue ?? TryCreateInstance(type, context);
        if (instance == null) // No need to log error, TryCreateInstance already logs.
            return default;

        foreach (XmlNode node in context.Node.ChildNodes)
        {
            if (node.NodeType != XmlNodeType.Element)
                continue;

            // Try to find member from name.
            var member = GetMember(instance.GetType(), node.Name);
            if (!member.IsValid)
            {
                DefDebugger.Error($"Failed to find member called '{node.Name}' in class '{instance.GetType().FullName}'!", context); // This technically isn't the right context...
                continue;
            }

            // Resolve type (uses member type unless overriden using Type="name")
            var childType = GetNodeType(node, member.Type);

            // Make child context.
            var ctx = new XmlParseContext
            {
                Loader = this,
                Node = node,
                TextValue = node.InnerText,
                DefaultType = member.Type,
                TargetType = childType,
                CurrentValue = member.GetValue(instance),
                Member = member,
                Owner = instance
            };

            // Parse recursively.
            var parsed = NodeToObject(ctx);

            // Assign value back unless it is null.
            if (parsed.ShouldWrite)
                member.SetValue(instance, parsed.Value);
        }

        return new ParseResult(instance);
    }

    private static object TryCreateInstance(Type type, in XmlParseContext context)
    {
        try
        {
            var instance = Activator.CreateInstance(type);

            if (instance is IPreXmlConstruct pre)
                pre.PreXmlConstruct(context);

            return instance;
        }
        catch (Exception e)
        {
            if (context.IsValid)
                DefDebugger.Error($"Failed to create instance of '{type.FullName}'.", context, e);
            else
                DefDebugger.Error($"Failed to create instance of '{type.FullName}'.", e);
            return null;
        }
    }

    internal MemberWrapper GetMember(Type type, string name) => GetMembers(type).GetMember(name);

    private bool IsListImplied(XmlNode node)
    {
        if (!node.HasChildNodes)
            return false;

        foreach (XmlNode child in node)
        {
            if (child.NodeType == XmlNodeType.Element && child.Name != Config.ListItemName)
                return false;
        }
        return true;
    }

    private static bool IsValueNode(XmlNode node)
    {
        if (!node.HasChildNodes)
            return false;

        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.NodeType)
            {
                // Ignore comments.
                case XmlNodeType.Comment:
                    continue;

                // Text is just the value.
                case XmlNodeType.Text:
                    continue;

                // Anything else means that it is not a simple name-value pair.
                default:
                    return false;
            }
        }

        return true;
    }

    private void Merge(XmlNode destination, XmlNode source)
    {
        bool shouldInherit = source.GetAttributeAsBool("Inherit", true);
        if (!shouldInherit)
        {
            var clone = source.CloneNode(true);
            clone.Attributes.RemoveNamedItem("Inherit");
            destination.ParentNode.ReplaceChild(destination, clone);
            return;
        }

        // Remove attributes that should not be inherited.
        foreach (var attr in doNotInheritAttributeNames)
        {
            var found = destination.Attributes[attr];
            if (found != null)
                destination.Attributes.Remove(found);
        }

        // Merge attributes.
        foreach (XmlAttribute attr in source.Attributes)
        {
            destination.SetAttribute(attr.Name, attr.Value);
        }

        // Is the target a simple value type then copy over the value.
        if (IsValueNode(destination))
        {
            destination.InnerText = source.InnerText;
            return;
        }

        // Should we be doing a regular merge or an append? Append is normally used for lists.
        bool shouldAppend = destination.GetAttributeAsBool("IsList", IsListImplied(source));

        // Merges nodes.
        foreach (XmlNode child in source)
        {
            if (child.NodeType != XmlNodeType.Element)
                continue;

            var dest = destination[child.Name];

            // Append if necessary.
            if (shouldAppend || dest == null)
            {
                var newChild = child.CloneNode(true);
                destination.AppendChild(newChild);
                continue;
            }

            // Replace (merge) mode.
            Merge(dest, child);
        }
    }

    private List<XmlNode> GetInheritance(XmlNode node)
    {
        var original = node;
        var root = GetRootNode(masterDoc);
        
        tempInheritance.Clear();
        tempInheritanceList.Clear();

        while (true)
        {
            if (!tempInheritance.Add(node))
            {
                DefDebugger.Error($"Cyclic inheritance detected in '{original.Name}' tree: {node.Name}. Def will not be loaded.");
                return null;
            }
            tempInheritanceList.Add(node);
            
            string parentName = node.GetAttributeValue("Parent");
            if (parentName == null)
            {
                tempInheritanceList.Reverse();
                return tempInheritanceList;
            }

            var found = root[parentName];
            if (found == null)
            {
                DefDebugger.Error($"Failed to find parent called '{parentName}' of '{node.Name}' for def '{original.Name}'. Def will not be loaded.");
                return null;
            }

            node = found;
        }
    }

    public void ResolveInheritance()
    {
        var root = GetRootNode(masterDoc);

        if (root == null)
        {
            DefDebugger.Error("Called ResolveInheritance when there are no xml documents loaded! Use AppendDocument before calling this.");
            return;
        }

        var toDelete = new List<XmlNode>();
        var created = new List<XmlNode>();

        foreach (XmlNode def in root.ChildNodes)
        {
            // Do not do inheritance for abstract types.
            if (def.GetAttributeAsBool("Abstract"))
                continue;

            var inheritance = GetInheritance(def);
            if (inheritance == null)
                continue;

            // No inheritance to do.
            if (inheritance.Count == 1)
                continue;

            // Clone base def, add to root.
            var baseDef = inheritance[0];
            var baseClone = CloneWithName(baseDef, def.Name);
            created.Add(baseClone);

            // Merge all parts of the inheritance tree back into that base clone.
            for (int i = 1; i < inheritance.Count; i++)
            {
                var part = inheritance[i];
                Merge(baseClone, part);
            }

            // Delete the def node, because the base clone has 'become' it.
            toDelete.Add(def);
        }

        foreach (var item in toDelete)
        {
            root.RemoveChild(item);
        }

        foreach (var toAdd in created)
        {
            root.AppendChild(toAdd);
        }

        HasResolvedInheritance = true;
    }

    private XmlNode CloneWithName(XmlNode toClone, string newName)
    {
        var created = masterDoc.CreateElement(newName);

        // Copy attributes.
        foreach (XmlAttribute attr in toClone.Attributes)
        {
            created.SetAttribute(attr.Name, attr.Value);
        }

        // Copy inner nodes.
        foreach (XmlNode inner in toClone)
        {
            created.AppendChild(inner.CloneNode(true));
        }

        return created;
    }

    public string GetMasterDocumentXml()
    {
        var stringBuilder = new StringBuilder();

        var element = XElement.Parse(masterDoc.InnerXml);

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = true,
            NewLineOnAttributes = false,
        };

        using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
        {
            element.Save(xmlWriter);
        }

        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        
    }

    #region Helper types
    public readonly ref struct ParseResult
    {
        public bool ShouldWrite => Value != null || ForceWrite;

        public object Value { get; init; }
        public bool ForceWrite { get; init; }

        public ParseResult(object value, bool forceWrite = false)
        {
            Value = value;
            ForceWrite = forceWrite;
        }
    }

    private enum NodeType
    {
        Default,
        List,
        Dictionary
    }
    #endregion
}