namespace Quest.Items;

public class Carrot : Consumable
{
    public Carrot(int amount, string? customName = null) : base(ItemTypes.Carrot, amount, 3, customName: customName)
    { }
}
