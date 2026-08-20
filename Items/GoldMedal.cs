namespace Quest.Items;

public class GoldMedal : Item
{
    public GoldMedal(int amount, string? customName = null) : base(ItemTypes.GoldMedal, amount, customName)
    { }
}
