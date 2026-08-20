namespace Quest.Items;

public class Cloth : Item
{
    public Cloth(int amount, string? customName = null) : base(ItemTypes.Cloth, amount, customName)
    { }
}
