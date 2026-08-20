namespace Quest.Items;

public class ActiveOrb : Item
{
    public ActiveOrb(int amount, string? customName = null) : base(ItemTypes.ActiveOrb, amount, customName)
    {
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        return true;
    }
}
