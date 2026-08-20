namespace Quest.Items;

public class Diamond : Item
{
    public Diamond(int amount, string? customName = null) : base(ItemTypes.Diamond, amount, customName)
    { }
}
