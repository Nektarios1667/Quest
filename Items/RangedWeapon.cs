namespace Quest.Items;

public class RangedWeapon : Item
{
    public float FireRate { get; } // Seconds between shots
    public float ProjectileSpeed { get; } // Tiles
    public ushort Damage { get; } // Projectile damage
    public TextureID ProjectileTexture { get; } // Texture for the projectile
    public ItemRef? Ammo { get; protected set; }
    public RangedWeapon(ItemType itemType, byte amount, float firerate, float tileSpeed, ushort damage, TextureID projectileTexture, string? customName = null) : base(itemType, amount, customName)
    {
        Amount = amount;
        FireRate = firerate;
        ProjectileSpeed = tileSpeed;
        Damage = damage;
        ProjectileTexture = projectileTexture;
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        // Check time
        if (!TimerManager.IsCompleteOrMissing($"RangedAttack_{UID}")) return false;

        // Use
        if (Ammo == null || player.Inventory.Has(Ammo))
        {
            Vector2 dir = InputManager.MousePosition.ToVector2() - Constants.Middle.ToVector2() - CameraManager.CameraOffset;
            // Player-owned projectiles have UID of 0
            Projectile projectile = new(gameManager, 0, CameraManager.CameraDest, (float)Math.Atan2(dir.Y, dir.X), ProjectileTexture, Damage, ProjectileSpeed);
            projectile.Position -= projectile.Size.ToVector2() / 2;
            gameManager.LevelManager.Level.Projectiles.Add(projectile);

            if (Ammo != null)
            {
                player.Inventory.Consume(Ammo, ignoreCheck: true);
                gameManager.OverlayManager.Notification($"-{Ammo.Amount} {Ammo.Name} ", Color.Red, 1f);
            }
            TimerManager.SetTimer($"RangedAttack_{UID}", FireRate, null);
            return true;
        }
        else
        {
            gameManager.OverlayManager.Notification($"No {Ammo.Name}!", Color.Red, 1f);
            TimerManager.SetTimer($"RangedAttack_{UID}", FireRate, null);
        }

        return false;
    }
}

