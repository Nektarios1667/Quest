namespace Quest.Items;

public class InactiveOrb : Item
{
    public InactiveOrb(int amount) : base(ItemTypes.InactiveOrb, amount)
    {
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        return true;
    }
}
