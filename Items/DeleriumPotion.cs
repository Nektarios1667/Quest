namespace Quest.Items;

public class DeleriumPotion : Consumable
{
    public DeleriumPotion(byte amount, string? customName = null) : base(ItemTypes.DeleriumPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Delerium, 1f, 30f), customName)
    { }
}
