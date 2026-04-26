namespace Quest.Items;
public class RegenerationPotion : Item
{
    public RegenerationPotion(byte amount, string? customName = null) : base(ItemTypes.RegenerationPotion, amount, customName)
    {}
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        player.StatusManager.AddStatusEffect(player, StatusEffect.Regeneration, 10);
        player.Inventory.Consume(GetItemRef());
        player.Inventory.AddItem(new(ItemTypes.GlassBottle, 1, CustomName));
        SoundManager.PlaySound("Gulp", pitchVariation: 0.25f);
    }
}
