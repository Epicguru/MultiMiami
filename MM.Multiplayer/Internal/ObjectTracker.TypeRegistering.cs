using MM.Logging;
using System.Reflection;
using System.Reflection.Emit;

namespace MM.Multiplayer.Internal;

public partial class ObjectTracker
{
    private static Func<object> GenerateFastConstructor(Type type)
    {
        var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);

        if (constructor == null)
        {
            Log.Error($"Failed to find no-parameter public constructor for type {type.FullName}");
            return null;
        }

        // Generate IL, make a func out of it.

        DynamicMethod method = new DynamicMethod("_StaticFastConstructor_", type, Type.EmptyTypes, type);

        ILGenerator il = method.GetILGenerator();
        il.DeclareLocal(type);
        il.Emit(OpCodes.Newobj, constructor);
        il.Emit(OpCodes.Ret);

        return (Func<object>)method.CreateDelegate(typeof(Func<object>));
    }

    public readonly struct TypeData
    {
        public bool IsValid => ID != 0;

        public readonly ushort ID;
        public readonly Func<object> Constructor;

        public TypeData(ushort id, Func<object> constructor)
        {
            ID = id;
            Constructor = constructor;
        }
    }

    private readonly Dictionary<Type, TypeData> typeToData = new Dictionary<Type, TypeData>(256);
    private readonly TypeData[] idToData = new TypeData[MAX_NET_OBJECT_TYPES];
    private ushort maxTypeID = 1;

    public bool IsTypeRegistered<T>() where T : NetObject => IsTypeRegistered(typeof(T));

    public bool IsTypeRegistered(Type type) => type != null && typeToData.ContainsKey(type);
    
    public TypeData GetTypeData(Type type) => type != null && typeToData.TryGetValue(type, out var found) ? found : default;

    public TypeData GetTypeData(ushort typeID) => idToData[typeID];

    public ushort RegisterType<T>() where T : NetObject => RegisterType(typeof(T));

    public ushort RegisterType(Type type)
    {
        if (type == null)
            return 0;

        var current = GetTypeData(type);
        if (current.IsValid)
            return current.ID;

        var func = GenerateFastConstructor(type);
        if (func == null)
            return 0;

        var newID = GetNewTypeID();
        var data = new TypeData(newID, func);

        typeToData.Add(type, data);
        idToData[newID] = data;
        return newID;
    }

    /// <summary>
    /// Creates a new instance of a registered net object type.
    /// The <see cref="CreateInstance{T}(ushort)"/> overload is preferred because it is faster.
    /// </summary>
    /// <typeparam name="T">The type of the created object. simply use <see cref="NetObject"/> if the type is not known at compile time.</typeparam>
    /// <returns>A new instance of the specified type, or null if the type <typeparamref name="T"/> has not been registered.</returns>
    public T CreateInstance<T>() where T : NetObject
    {
        if (typeToData.TryGetValue(typeof(T), out var found))
            return found.Constructor() as T;
        return null;
    }

    /// <summary>
    /// Creates a new instance of a registered net object type.
    /// This overload is faster than calling <see cref="CreateInstance{T}()"/>
    /// because it avoids a dictionary lookup.
    /// </summary>
    /// <typeparam name="T">The type of the created object. simply use <see cref="NetObject"/> if the type is not known at compile time.</typeparam>
    /// <param name="id">The type ID of the object.</param>
    /// <returns>A new instance of the specified type, or null if the <paramref name="id"/> is invalid.</returns>
    public T CreateInstance<T>(ushort id) where T : NetObject
    {
        var found = idToData[id];
        if (found.IsValid)
            return found.Constructor() as T;
        return null;
    }

    private ushort GetNewTypeID() => maxTypeID++;
}
