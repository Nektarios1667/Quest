
namespace Quest.Items;

public class Crossbow : RangedWeapon
{
    public Crossbow(byte amount, string? customName = null) : base(ItemTypes.Crossbow, amount, customName)
    {
        // Configure ranged weapon properties here instead of passing them to base
        FireRate = 1.8f;
        ProjectileSpeed = 13f;
        Damage = 25;
        ProjectileTexture = TextureID.ArrowProjectile;
        AccuracyAngle = 3 * MathF.PI / 180f;

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
