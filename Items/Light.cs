namespace Quest.Items;

public class Light : Item
{
    public int LightStrength { get; protected set; }
    public Light(ItemType itemType, int amount, string? name = null) : base(itemType, amount, name)
    {
        LightStrength = 8;
    }
}
