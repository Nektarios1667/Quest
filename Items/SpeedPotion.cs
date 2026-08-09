namespace Quest.Items;

public class SpeedPotion : Consumable
{
    public SpeedPotion(byte amount, string? customName = null) : base(ItemTypes.SpeedPotion, amount, 0, new(ItemTypes.GlassBottle, 1), (StatusEffect.Speed, 1f, 30f), customName)
    { }
}
