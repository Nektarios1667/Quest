namespace Quest.Items;
public class InactivePalantir : Item
{
    public InactivePalantir(int amount) : base(ItemTypes.InactivePalantir, amount)
    {
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        SoundManager.PlaySound("Spook");
    }
}
