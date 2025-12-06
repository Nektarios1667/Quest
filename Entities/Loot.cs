namespace Quest.Entities;
public struct Loot
{
    public ushort UID { get; } = UIDManager.Get(UIDCategory.Loot);
    public readonly string DisplayName => $"{Amount} {StringTools.FillCamelSpaces(Item)}";
    public string Item { get; private set; }
    public byte Amount { get; set; }
    public Point Location { get; set; }
    public TextureID Texture { get; private set; }
    public float Birth { get; private set; }
    public Loot(string item, byte amount, Point location, float time)
    {
        Item = item;
        Amount = amount;
        Location = location;
        Texture = ParseTextureString(item);
        Birth = time;
    }
    public void Dispose()
    {
        UIDManager.Release(UIDCategory.Loot, UID);
    }
}