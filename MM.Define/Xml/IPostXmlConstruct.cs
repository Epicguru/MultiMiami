namespace MM.Define.Xml;

/// <summary>
/// An interface with a single callback method that is invoked
/// after the implementing object has been parsed from XML and all it's containing
/// members have been populated.
/// </summary>
public interface IPostXmlConstruct
{
    void PostXmlConstruct(in XmlParseContext context);
}