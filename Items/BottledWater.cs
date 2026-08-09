namespace Quest.Items;

public class BottledWater : Consumable
{
    public BottledWater(int amount, string? customName = null) : base(ItemTypes.BottledWater, amount, 4, leftover: new ItemRef(ItemTypes.GlassBottle, 1), customName: customName)
    {
    }
}
