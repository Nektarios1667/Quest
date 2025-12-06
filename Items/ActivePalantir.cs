namespace Quest.Items;
public class ActivePalantir : Item
{
    public ActivePalantir(int amount) : base(ItemTypes.ActivePalantir, amount)
    {
    }
    public override void PrimaryUse(PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        player.Inventory.ReplaceItem(this, new InactivePalantir(1));
    }
}
