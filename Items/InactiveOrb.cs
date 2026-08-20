namespace Quest.Items;

public class InactiveOrb : Item
{
    public InactiveOrb(int amount, string? customName = null) : base(ItemTypes.InactiveOrb, amount, customName)
    {
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        return true;
    }
}
