namespace Quest.Items;

public class GoldKey : Item
{
    public GoldKey(int amount, string? customName = null) : base(ItemTypes.GoldKey, amount, customName)
    {
    }
}
