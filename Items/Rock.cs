namespace Quest.Items;

public class Rock : Item
{
    public Rock(int amount, string? customName = null) : base(ItemTypes.Rock, amount, customName)
    {
    }
}
