namespace Quest.Items;

public class ProtectionPotion : Consumable
{
    public ProtectionPotion(byte amount, string? customName = null) : base(ItemTypes.ProtectionPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Protection, 1f, 30f), customName)
    { }
}
