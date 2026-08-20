namespace Quest.Items;

public class FireStaff : RangedWeapon
{
    public FireStaff(int amount, string? customName = null) : base(ItemTypes.FireStaff, amount, customName)
    {
        FireRate = 2f;
        ProjectileSpeed = 6f;
        Damage = 35;
        ProjectileTexture = TextureID.Fireball;
        AccuracyAngle = 2 * MathF.PI / 180f;
    }

}
