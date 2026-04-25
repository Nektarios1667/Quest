namespace Quest.Items;
public class Arrow : Item
{
    public Arrow(byte amount, string? customName = null) : base(ItemTypes.Arrow, amount, customName)
    {}
}
