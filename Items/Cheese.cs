namespace Quest.Items;

public class Cheese : Consumable
{
    public Cheese(int amount, string? customName = null) : base(ItemTypes.Cheese, amount, 7, customName: customName)
    {
    }
}
