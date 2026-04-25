namespace Quest.Items;
public class SteelSword : MeleeWeapon
{
    public SteelSword(byte amount, string? customName = null) : base(ItemTypes.SteelSword, amount, 1.4f, 45, 20, customName)
    {
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        base.PrimaryUse(gameManager, player);
        SoundManager.PlaySound("Swoosh", pitchVariation: 0.25f);
    }
}
