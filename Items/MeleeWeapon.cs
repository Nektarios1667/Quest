namespace Quest.Items;
public class MeleeWeapon : Item, IProjectileOwner
{
    public TextureID ProjectileTexture => TextureID.Slash;
    public ushort ProjectileSpeed => 0; // Unused since melee weapons don't have actual projectiles, but required by IProjectileOwner
    public float FireRate { get; } // Seconds between swings
    public int Range { get; } // Pixels
    public ushort Damage { get; } // Damage dealt per hit
    public MeleeWeapon(ItemType itemType, byte amount, float firerate, int range, ushort damage, string? customName = null) : base(itemType, amount, customName)
    {
        Amount = amount;
        FireRate = firerate;
        Range = range;
        Damage = damage;
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing($"MeleeAttack_{UID}"))
        {
            // Positioning and aiming
            Vector2 dir = InputManager.MousePosition.ToVector2() - Constants.Middle.ToVector2() - CameraManager.CameraOffset;
            Projectile projectile = new Projectile(gameManager, player, CameraManager.CameraDest, (float)Math.Atan2(dir.Y, dir.X), size: new(Range));
            projectile.Position -= projectile.Size.ToVector2() / 2;
            projectile.Position += dir.NormalizedCopy() * (Range / 2);

            gameManager.LevelManager.Level.Projectiles.Add(projectile);
            TimerManager.SetTimer($"MeleeAttackDecay_{UID}", FireRate / 2, projectile.Destroy);

            TimerManager.SetTimer($"MeleeAttack_{UID}", FireRate, null);
        }
    }
}

