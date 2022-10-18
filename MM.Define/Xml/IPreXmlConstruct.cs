namespace MM.Define.Xml;

/// <summary>
/// An interface with a single callback method that is invoked
/// as soon as the implementing object has been instantiated, before any of its members
/// have been instantiated. 
/// </summary>
public interface IPreXmlConstruct
{
    void PreXmlConstruct(in XmlParseContext context);
}