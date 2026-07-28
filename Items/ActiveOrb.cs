namespace Quest.Items;

public class ActiveOrb : Item
{
    public ActiveOrb(int amount) : base(ItemTypes.ActiveOrb, amount)
    {
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        return true;
    }
}
