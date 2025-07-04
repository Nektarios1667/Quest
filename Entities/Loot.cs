namespace Quest.Entities;
public struct Loot
{
    public int UID { get; } = UIDManager.NewUID("Loot");
    public readonly string DisplayName => $"{Amount} {StringTools.FillCamelSpaces(Item)}";
    public string Item { get; private set; }
    public int Amount { get; private set; }
    public Point Location { get; set; }
    public TextureID Texture { get; private set; }
    public float Birth { get; private set; }
    public Loot(string item, int amount, Point location, float time)
    {
        Item = item;
        Amount = amount;
        Location = location;
        Texture = ParseTextureString(item);
        Birth = time;
    }
}