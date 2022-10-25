using ImGuiNET;
using System.Reflection;

namespace MM.DearImGui;

public static class DebugReadoutDrawer
{
    private struct Container
    {
        public string Name;
        public Func<string> MakeString;
    }

    private static readonly Dictionary<string, List<Container>> containers = new Dictionary<string, List<Container>>();

    private static List<Container> GetContainerFor(string category)
    {
        category ??= "General";
        if (containers.TryGetValue(category, out var list))
            return list;

        list = new List<Container>();
        containers.Add(category, list);
        return list;
    }

    public static void RegisterAssembly(Assembly a)
    {
        if (a == null)
            throw new ArgumentNullException(nameof(a));

        foreach (var type in a.GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = field.GetCustomAttribute<DebugReadoutAttribute>();
                if (attr == null)
                    continue;

                GetContainerFor(attr.Category).Add(new Container
                {
                    MakeString = () => field.GetValue(null)?.ToString() ?? "<null>",
                    Name = attr.Name ?? field.Name
                });
            }

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (prop.GetMethod == null)
                    continue;

                var attr = prop.GetCustomAttribute<DebugReadoutAttribute>();
                if (attr == null)
                    continue;

                GetContainerFor(attr.Category).Add(new Container
                {
                    MakeString = () => prop.GetValue(null)?.ToString() ?? "<null>",
                    Name = attr.Name ?? prop.Name
                });
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<DebugReadoutAttribute>();
                if (attr == null)
                    continue;

                if (method.IsAbstract)
                    continue;
                if (method.ReturnType == typeof(void))
                    continue;
                if (method.ContainsGenericParameters)
                    continue;
                if (method.GetParameters().Length > 0)
                    continue;

                GetContainerFor(attr.Category).Add(new Container
                {
                    MakeString = () => method.Invoke(null, Array.Empty<object>())?.ToString() ?? "<null>",
                    Name = attr.Name ?? method.Name
                });
            }
        }
    }

    public static void DrawWindow()
    {
        if (!ImGui.Begin("Debug Values", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            return;

        foreach (var pair in containers)
        {
            var cat = pair.Key;
            var list = pair.Value;

            if (ImGui.CollapsingHeader(cat, ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var item in list)
                {
                    string value;
                    try
                    {
                        value = item.MakeString() ?? "<null>";
                    }
                    catch (Exception e)
                    {
                        value = $"<{e.GetType().Name}:{e.Message}>";
                    }

                    ImGui.Text($"{item.Name}: {value}");
                }
            }
        }

        ImGui.End();
    }
}