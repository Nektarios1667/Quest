namespace Quest.Items;

public class CookedFish : Consumable
{
    public CookedFish(int amount, string? customName = null) : base(ItemTypes.CookedFish, amount, 11, customName: customName)
    { }
}
