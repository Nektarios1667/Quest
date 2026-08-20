namespace Quest.Items;

public class Gold : Item
{
    public Gold(int amount, string? customName = null) : base(ItemTypes.Gold, amount, customName)
    { }
}
