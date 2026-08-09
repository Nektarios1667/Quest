namespace Quest.Items;

public class RawFish : Consumable
{
    public RawFish(int amount, string? customName = null) : base(ItemTypes.RawFish, amount, 8, customName: customName)
    { }
}
