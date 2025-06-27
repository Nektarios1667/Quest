namespace Quest.Items;
public class ActivePalantir : Item
{
    public ActivePalantir(PlayerManager playerManager, int amount) : base(playerManager, amount)
    {
        MaxAmount = 1;
        Description = "A seeing stone used to communicate with sauron.";
    }
    public override void PrimaryUse()
    {
        SoundManager.PlaySound("Spook");
    }
}
