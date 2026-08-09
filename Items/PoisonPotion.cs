namespace Quest.Items;

public class PoisonPotion : Consumable
{
    public PoisonPotion(byte amount, string? customName = null) : base(ItemTypes.PoisonPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Poison, 1f, 10f), customName)
    { }
}
