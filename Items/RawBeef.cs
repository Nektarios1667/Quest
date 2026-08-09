namespace Quest.Items;

public class RawBeef : Consumable
{
    public RawBeef(int amount, string? customName = null) : base(ItemTypes.RawBeef, amount, 1, customName: customName)
    { }
}
