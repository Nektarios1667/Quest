namespace Quest.Entities;
public struct Loot
{
    public Item Item { get; private set; }
    public Point Location { get; set; }
    public TextureID Texture { get; private set; }
    public float Birth { get; private set; }
    public Loot(Item item, Point location, float time)
    {
        Item = item;
        Location = location;
        Texture = ParseTextureString(item.Name);
        Birth = time;
    }
}