namespace Quest.Items;
public class InactivePalantir : Item
{
    public InactivePalantir(PlayerManager playerManager, int amount) : base(playerManager, amount)
    {
        MaxAmount = 1;
        Description = "A seeing stone used to communicate with sauron.";
    }
    public override void PrimaryUse()
    {
        SoundManager.PlaySound("Spook");
        PlayerManager.Inventory.ReplaceItem(this, new ActivePalantir(PlayerManager, 1));
    }
}
