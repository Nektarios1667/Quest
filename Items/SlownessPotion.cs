namespace Quest.Items;

public class SlownessPotion : Consumable
{
    public SlownessPotion(byte amount, string? customName = null) : base(ItemTypes.SlownessPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Slowness, 1f, 30f), customName)
    { }
}
