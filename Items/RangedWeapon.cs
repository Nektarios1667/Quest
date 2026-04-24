namespace Quest.Items;
public class RangedWeapon : Item, IProjectileOwner
{
    public float FireRate { get; } // Seconds between shots
    public ushort ProjectileSpeed { get; } // Pixels
    public ushort Damage { get; } // Projectile damage
    public TextureID ProjectileTexture { get; } // Texture for the projectile
    public RangedWeapon(ItemType itemType, byte amount, float firerate, ushort speed, ushort damage, TextureID projectileTexture, string? customName = null) : base(itemType, amount, customName)
    {
        Amount = amount;
        FireRate = firerate;
        ProjectileSpeed = speed;
        Damage = damage;
        ProjectileTexture = projectileTexture;
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing($"RangedAttack_{UID}"))
        {
            Vector2 dir = InputManager.MousePosition.ToVector2() - Constants.Middle.ToVector2() - CameraManager.CameraOffset;
            Projectile projectile = new Projectile(gameManager, player, CameraManager.CameraDest, (float)Math.Atan2(dir.Y, dir.X));
            projectile.Position -= projectile.Size.ToVector2() / 2;
            gameManager.LevelManager.Level.Projectiles.Add(projectile);

            TimerManager.SetTimer($"RangedAttack_{UID}", FireRate, null);
        }
    }
}

