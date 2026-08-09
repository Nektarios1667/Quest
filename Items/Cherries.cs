namespace Quest.Items;

public class Cherries : Consumable
{
    public Cherries(int amount, string? customName = null) : base(ItemTypes.Cherries, amount, 2, customName: customName)
    {
    }
}
