namespace Quest.Managers;
public static class UIDManager
{
    private static Dictionary<string, int> uids = new()
    {
        { "Items", 0 },
        { "Enemies", 0 },
        { "Tiles", 0 },
        { "Loot", 0 },
    };
    public static void NewUIDCategory(string name)
    {
        if (uids.ContainsKey(name))
            throw new ArgumentException($"UID with name '{name}' already exists");
        uids[name] = 0;
    }
    public static void TryNewUIDCategory(string name)
    {
        if (!uids.ContainsKey(name))
            uids[name] = 0;
    }
    public static int NewUID(string category)
    {
        if (!uids.TryGetValue(category, out int value))
            throw new KeyNotFoundException($"No UID category with name '{category}' found");
        uids[category] = ++value;
        return value;
    }
    public static bool IsUIDUsed(int uid, string category)
    {
        if (!uids.TryGetValue(category, out int value))
            throw new KeyNotFoundException($"No UID category with name '{category}' found");
        return value >= uid && uid > 0;
    }
}
