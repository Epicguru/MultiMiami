using Ceras;

namespace MM.Define.Ceras;

public sealed class CerasCache
{
    public static CerasCache TryLoad(CerasSerializer serializer, string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            return serializer.Deserialize<CerasCache>(bytes);
        }
        catch
        {
            // Ignored
            return null;
        }
    }

    public uint Version;
    public List<IDef> AllDefs = new List<IDef>();

    public void Save(CerasSerializer serializer, string filePath)
    {
        File.WriteAllBytes(filePath, serializer.Serialize(this));
    }
}