namespace Quest.Items;
public class ActivePalantir : Item
{
    public ActivePalantir(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A seeing stone used to communicate with sauron.";
    }
    public override void PrimaryUse(PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        player.Inventory.ReplaceItem(this, new InactivePalantir(1));
    }
}
