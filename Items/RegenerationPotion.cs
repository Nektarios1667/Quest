namespace Quest.Items;

public class RegenerationPotion : Consumable
{
    public RegenerationPotion(byte amount, string? customName = null) : base(ItemTypes.RegenerationPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Regeneration, 1f, 10f), customName)
    { }
}
