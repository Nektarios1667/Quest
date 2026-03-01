namespace Quest.Items;

public class Light : Item
{
    public int LightStrength { get; protected set; }
    public Color LightColor { get; protected set; } = Color.Transparent;
    public Light(ItemType itemType, int amount, string? name = null) : base(itemType, amount, name)
    {
        LightStrength = 8;
    }
}
