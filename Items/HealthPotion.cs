namespace Quest.Items;
public class HealthPotion : Item
{
    public HealthPotion(byte amount, string? customName = null) : base(ItemTypes.HealthPotion, amount, customName)
    {}
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (player.Health >= player.MaxHealth) return;

        player.Heal(gameManager, 20);
        player.Inventory.Consume(GetItemRef());
        player.Inventory.AddItem(new(ItemTypes.GlassBottle, 1, CustomName));
    }
}
