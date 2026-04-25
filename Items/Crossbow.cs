
namespace Quest.Items;
public class Crossbow : RangedWeapon
{
    public Crossbow(byte amount, string? customName = null) : base(ItemTypes.Crossbow, amount, 1.8f, 800, 25, TextureID.ArrowProjectile, customName)
    {
         Ammo = new(ItemTypes.Arrow, 1);
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        base.PrimaryUse(gameManager, player);
        SoundManager.PlaySound("Bow", 0.7f, pitchVariation: 0.2f);
    }
}
