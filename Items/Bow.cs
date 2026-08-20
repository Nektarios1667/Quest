
namespace Quest.Items;

public class Bow : RangedWeapon
{
    public Bow(int amount, string? customName = null) : base(ItemTypes.Bow, amount, customName)
    {
        FireRate = 1.2f;
        ProjectileSpeed = 10f;
        Damage = 15;
        ProjectileTexture = TextureID.ArrowProjectile;
        AccuracyAngle = 5 * MathF.PI / 180f;

        Ammo = new(ItemTypes.Arrow, 1);
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        bool success = base.PrimaryUse(gameManager, player);
        if (success)
            SoundManager.PlaySound("Bow", 0.7f, pitchVariation: 0.2f);
        return success;
    }
}
