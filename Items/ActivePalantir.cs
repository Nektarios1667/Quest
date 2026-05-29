namespace Quest.Items;
public class ActivePalantir : Item
{
    public ActivePalantir(int amount) : base(ItemTypes.ActivePalantir, amount)
    {
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        return true;
    }
}
