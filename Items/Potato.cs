namespace Quest.Items;

public class Potato : Consumable
{
    public Potato(int amount, string? customName = null) : base(ItemTypes.Potato, amount, 6, customName: customName)
    { }
}
