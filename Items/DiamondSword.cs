namespace Quest.Items;
public class DiamondSword : MeleeWeapon
{
    public DiamondSword(byte amount, string? customName = null) : base(ItemTypes.DiamondSword, amount, 1.2f, 60, 40, customName)
    {
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        base.PrimaryUse(gameManager, player);
        SoundManager.PlaySound("Swoosh", pitchVariation: 0.25f);
    }
}
