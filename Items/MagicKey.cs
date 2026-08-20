namespace Quest.Items;

public class MagicKey : Item
{
    public MagicKey(int amount, string? customName = null) : base(ItemTypes.MagicKey, amount, customName)
    {
    }
}
