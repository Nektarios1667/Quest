namespace Quest.Items;

public class StrengthPotion : Consumable
{
    public StrengthPotion(byte amount, string? customName = null) : base(ItemTypes.StrengthPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Strength, 1f, 30f), customName)
    { }
}
