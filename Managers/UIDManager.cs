namespace Quest.Managers;

public enum UIDCategory
{
    Items,
    Loot,
    Enemies,
}

public static class UIDManager
{
    private static readonly Queue<ushort>[] uids;
    private static readonly ushort[] uidCounters;
    static UIDManager()
    {
        int categories = Enum.GetNames(typeof(UIDCategory)).Length;
        uids = new Queue<ushort>[categories];
        uidCounters = new ushort[categories];
        for (int i = 0; i < categories; i++)
        {
            uids[i] = new Queue<ushort>();
            uidCounters[i] = 0;
        }
    }
    public static ushort Get(UIDCategory category)
    {
        int c = (int)category;
        if (uids[c].Count > 0)
            return uids[c].Dequeue();
        else
            return uidCounters[c]++;
    }
    public static ushort Peek(UIDCategory category)
    {
        int c = (int)category;
        if (uids[c].Count > 0)
            return uids[c].Peek();
        else
            return uidCounters[c];
    }
    public static int Available(UIDCategory category)
    {
        int c = (int)category;
        return uidCounters[c] - uids[c].Count;
    }
    public static void Release(UIDCategory category, ushort uid)
    {
        uids[(int)category].Enqueue(uid);
    }
    public static void ReleaseAll(UIDCategory category)
    {
        int c = (int)category;
        uids[c].Clear();
        uidCounters[c] = 0;
    }
}
