namespace Quest.Utilities;
public struct LevelPath
{
    public string WorldName { get; private set; }
    public string LevelName { get; private set; }
    public string Path => ToString();
    public LevelPath(string path)
    {
        var parts = path.Split('\\', '/');
        if (parts.Length != 2)
        {
            WorldName = "NUL_WORLD";
            LevelName = "NUL_LEVEL";
            return;
        }
        WorldName = parts[0];
        LevelName = parts[1];
    }
    public LevelPath(string worldName, string levelName)
    {
        WorldName = worldName;
        LevelName = levelName;
    }
    public readonly bool IsNull() => WorldName == "NUL_WORLD" && LevelName == "NUL_LEVEL";
    public override readonly string ToString() => $"{WorldName}/{LevelName}";
}
