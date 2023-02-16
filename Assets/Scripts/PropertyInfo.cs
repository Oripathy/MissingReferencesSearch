using Object = UnityEngine.Object;

public class PropertyInfo
{
    public string Name { get; }
    public string Type { get; }
    public string Path { get; }
    public Object Object { get; }

    public PropertyInfo(string name, string type, string path, Object o)
    {
        Name = name;
        Type = type;
        Path = path;
        Object = o;
    }
}