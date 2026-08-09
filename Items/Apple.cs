namespace Quest.Items;

public class Apple : Consumable
{
    public Apple(int amount, string? customName = null) : base(ItemTypes.Apple, amount, 4, customName: customName)
    {
    }
}
