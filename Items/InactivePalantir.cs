namespace Quest.Items;
public class InactivePalantir : Item
{
    public InactivePalantir(int amount) : base(ItemTypes.InactivePalantir, amount)
    {
    }
    public override void PrimaryUse(PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        player.Inventory.ReplaceItem(this, new ActivePalantir(1));
    }
}
