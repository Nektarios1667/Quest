namespace Quest.Items;
public class StrengthPotion : Item
{
    public StrengthPotion(byte amount, string? customName = null) : base(ItemTypes.StrengthPotion, amount, customName)
    {}
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        player.StatusManager.AddStatusEffect(player, StatusEffect.Strength, 30);
        player.Inventory.Consume(GetItemRef());
        player.Inventory.AddItem(new(ItemTypes.GlassBottle, 1, CustomName));
        SoundManager.PlaySound("Gulp", pitchVariation: 0.25f);
    }
}
