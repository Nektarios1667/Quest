namespace Quest.Items;
public class InactivePalantir : Item
{
    public InactivePalantir(int amount) : base(ItemTypes.InactivePalantir, amount)
    {
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
        return true;
    }
}
