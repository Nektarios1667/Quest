namespace Quest.Items;

public class LightItem : Item
{
    public int LightStrength { get; protected set; }
    public LightItem(ItemType itemType, int amount, string? name = null) : base(itemType, amount, name)
    {
        LightStrength = 8;
    }
}
