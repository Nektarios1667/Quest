namespace Quest.Items;

public class Bread : Consumable
{
    public Bread(int amount, string? customName = null) : base(ItemTypes.Bread, amount, 9, customName: customName)
    {
    }
}
