namespace Quest.Items;

public class WeaknessPotion : Consumable
{
    public WeaknessPotion(byte amount, string? customName = null) : base(ItemTypes.WeaknessPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Weakness, 1f, 30f), customName)
    { }
}
