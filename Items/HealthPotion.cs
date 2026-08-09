namespace Quest.Items;

public class HealthPotion : Consumable
{
    public HealthPotion(byte amount, string? customName = null) : base(ItemTypes.HealthPotion, amount, 0, new(ItemTypes.GlassBottle, 1), null, customName)
    { }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (player.Health >= player.MaxHealth) return false;

        player.Heal(gameManager, 20);
        return base.PrimaryUse(gameManager, player);
    }
}
