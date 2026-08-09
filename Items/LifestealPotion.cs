namespace Quest.Items;

public class LifestealPotion : Consumable
{
    public LifestealPotion(byte amount, string? customName = null) : base(ItemTypes.LifestealPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Lifesteal, 1f, 30f), customName)
    { }
}
