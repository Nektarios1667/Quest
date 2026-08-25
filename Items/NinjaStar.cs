namespace Quest.Items;

public class NinjaStar : RangedWeapon
{
    public NinjaStar(int amount, string? customName = null) : base(ItemTypes.NinjaStar, amount, customName)
    {
        FireRate = 0.7f;
        ProjectileSpeed = 5f;
        Damage = 20;
        ProjectileTexture = TextureID.NinjaStarProjectile;
        AccuracyAngle = 10 * MathF.PI / 180f;

        Ammo = new ItemRef(ItemTypes.NinjaStar, 1);
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        bool success = base.PrimaryUse(gameManager, player);
        if (success)
            SoundManager.PlaySound("Swoosh", pitch: 1.1f, pitchVariation: 0.1f);
        return success;
    }
}
