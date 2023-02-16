public struct AssetInfo
{
    public string Name { get; }
    public string Path { get; }

    public AssetInfo(string name, string path)
    {
        Name = name;
        Path = path;
    }
}