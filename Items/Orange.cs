namespace Quest.Items;

public class Orange : Consumable
{
    public Orange(int amount, string? customName = null) : base(ItemTypes.Orange, amount, 5, customName: customName)
    {
    }
}
