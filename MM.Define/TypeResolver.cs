using System.Reflection;

namespace MM.Define;

public static class TypeResolver
{
    public static IEnumerable<Assembly> Assemblies { get; set; }
    
    private static readonly Dictionary<string, Type> cache = new Dictionary<string, Type>();

    public static Type Get(string name)
    {
        if (cache.TryGetValue(name, out var found))
            return found;

        // Search for assembly qualified.
        found = Type.GetType(name, false, false);
        if (found != null)
        {
            cache.Add(name, found);
            return found;
        }

        var all = Assemblies ?? AppDomain.CurrentDomain.GetAssemblies();

        // Search for full name.
        foreach (var ass in all)
        {
            found = ass.GetType(name, false, false);
            if (found != null)
            {
                cache.Add(name, found);
                return found;
            }    
        }

        // Search for short name.
        foreach (var ass in all)
        {
            foreach (var type in ass.GetTypes())
            {
                if (type.Name == name)
                {
                    cache.Add(name, type);
                    return type;
                }
            }
        }

        // Not found, cache null to avoid expensive search in future.
        cache.Add(name, null);
        return null;
    }
}