namespace Quest.Items;

public class CookedBeef : Consumable
{
    public CookedBeef(int amount, string? customName = null) : base(ItemTypes.CookedBeef, amount, 13, customName: customName)
    { }
}
