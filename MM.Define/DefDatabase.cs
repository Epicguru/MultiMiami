using System.Xml;
using MM.Define.Xml;

namespace MM.Define;

public static class DefDatabase
{
    public static int Count => allDefs.Count;

    public static XmlLoader Loader { get; private set; }

    private static readonly Dictionary<string, IDef> idToDef = new Dictionary<string, IDef>(4096);
    private static readonly List<IDef> allDefs = new List<IDef>(4096);
    private static readonly Dictionary<Type, DefContainer> defsOfType = new Dictionary<Type, DefContainer>(128);
    private static bool isReload;

    public static void Clear()
    {
        idToDef.Clear();
        allDefs.Clear();
        defsOfType.Clear();
    }

    public static void StartLoading(DefLoadConfig config, bool reloading = false)
    {
        if (Loader != null)
            throw new Exception("The loading process has already been started.");
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (!reloading)
            Clear();

        isReload = reloading;
        Loader = new XmlLoader(config);
    }

    public static void AddDefDocument(XmlDocument document, string source)
    {
        if (Loader == null)
            throw new Exception("The loading process has not been started, call StartLoading() first.");

        if (Loader.HasResolvedInheritance)
            throw new Exception("Cannot add more documents because FinishingAddingDefs() has been called.");

        if (document != null)
            Loader.AppendDocument(document, source);
    }

    public static void FinishingAddingDefs()
    {
        if (Loader == null)
            throw new Exception("The loading process has not been started, call StartLoading() first.");

        if (Loader.HasResolvedInheritance)
            return;

        Loader.ResolveInheritance();
    }

    public static void ApplyPatches(XmlDocument document, string source)
    {
        if (Loader == null)
            throw new Exception("The loading process has not been started, call StartLoading() first.");

        if (!Loader.HasResolvedInheritance)
            throw new Exception("To begin applying patches, call FinishAddingDefs() first.");

        if (document != null)
            Loader.ApplyPatches(document, source);
    }

    public static void FinishLoading()
    {
        if (Loader == null)
            throw new Exception("The loading process has not been started, call StartLoading() first.");

        // Resolve inheritance if not done already.
        if (!Loader.HasResolvedInheritance)
            Loader.ResolveInheritance();

        Func<string, IDef> existing = null;
        if (isReload)
            existing = str => idToDef.TryGetValue(str, out var found) ? found : null;

        // Parse defs.
        foreach (var def in Loader.MakeDefs(existing))
        {
            if (isReload)
                continue;

            Register(def);
        }

        // Save to ceras cache. TODO!

        DoPostLoadCallbacks();

        Loader = null;
    }

    private static void DoPostLoadCallbacks()
    {
        // Post-load.
        foreach (var def in allDefs)
        {
            try
            {
                def.PostLoad();
            }
            catch (Exception e)
            {
                DefDebugger.Error($"Exception PostLoading def '{def.ID}'.", e);
            }
        }

        // Late Post-load.
        foreach (var def in allDefs)
        {
            try
            {
                def.LatePostLoad();
            }
            catch (Exception e)
            {
                DefDebugger.Error($"Exception PostLoading def '{def.ID}'.", e);
            }
        }

        // Post-load.
        var reporter = new ConfigErrorReporter();
        foreach (var def in allDefs)
        {
            try
            {
                reporter.CurrentDef = def;
                def.ConfigErrors(reporter);
            }
            catch (Exception e)
            {
                DefDebugger.Error($"Exception PostLoading def '{def.ID}'.", e);
            }
        }
    }

    public static T Get<T>(string id) where T : class, IDef => idToDef.TryGetValue(id, out var found) ? found as T : null;

    public static bool Register(IDef def)
    {
        if (def?.ID == null)
            return false;

        if (!idToDef.TryAdd(def.ID, def))
            return false;

        allDefs.Add(def);

        Type t = def.GetType();
        while (t != null)
        {
            if (t != typeof(object))
                GetContainerForType(t).Add(def);
            t = t.BaseType;
        }

        return true;
    }

    public static bool UnRegister(IDef def)
    {
        if (def?.ID == null)
            return false;

        if (!idToDef.TryGetValue(def.ID, out var found) || found != def)
            return false;

        idToDef.Remove(def.ID);
        allDefs.Remove(def);
        GetContainerForType(def.GetType()).Remove(def);
        return true;
    }

    public static IReadOnlyList<IDef> GetAll() => allDefs;

    public static IReadOnlyList<T> GetAll<T>() where T : class, IDef
    {
        if (defsOfType.TryGetValue(typeof(T), out var found))
            return (found as DefContainer<T>).Defs;

        return Array.Empty<T>();
    }

    private static DefContainer GetContainerForType(Type type)
    {
        if (defsOfType.TryGetValue(type, out var found))
            return found;

        var created = Activator.CreateInstance(typeof(DefContainer<>).MakeGenericType(type)) as DefContainer;
        defsOfType.Add(type, created);
        return created;
    }

    #region Container classes
    private abstract class DefContainer
    {
        public abstract void Add(IDef def);
        public abstract void Remove(IDef def);
    }

    private sealed class DefContainer<T> : DefContainer where T : class, IDef
    {
        public readonly List<T> Defs = new List<T>();

        public override void Add(IDef def) => Defs.Add(def as T);
        public override void Remove(IDef def) => Defs.Remove(def as T);
    }
    #endregion
}
