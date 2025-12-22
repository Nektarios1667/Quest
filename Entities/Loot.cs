namespace Quest.Entities;
public struct Loot
{
    public ushort UID { get; } = UIDManager.Get(UIDCategory.Loot);
    public ItemRef Item { get; }
    public Point Location { get; set; }
    public float Birth { get; private set; }
    // Generated
    public readonly TextureID Texture => Item.Texture;
    public readonly string DisplayName => $"{Item.Amount} {StringTools.FillCamelSpaces(Item.Name)}";
    public Loot(ItemRef item, Point location, float time)
    {
        Item = item;
        Location = location;
        Birth = time;
    }
    public readonly void Dispose()
    {
        UIDManager.Release(UIDCategory.Loot, UID);
    }
}