
namespace Quest.Items;

public class Slingshot : RangedWeapon
{
    public Slingshot(int amount, string? customName = null) : base(ItemTypes.Slingshot, amount, customName)
    {
        FireRate = 0.8f;
        ProjectileSpeed = 7f;
        Damage = 10;
        ProjectileTexture = TextureID.RockProjectile;
        AccuracyAngle = 8 * MathF.PI / 180f;

        Ammo = new(ItemTypes.Rock, 1);
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        bool success = base.PrimaryUse(gameManager, player);
        if (success)
            SoundManager.PlaySound("Bow", 0.7f, pitchVariation: 0.2f);
        return success;
    }
}
