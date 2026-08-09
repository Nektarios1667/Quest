namespace Quest.Items;

public class Chicken : Consumable
{
    public Chicken(int amount, string? customName = null) : base(ItemTypes.Chicken, amount, 10, customName: customName)
    {
    }
}
