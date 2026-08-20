namespace Quest.Items;

public class RangedWeapon : Item
{
    public float AccuracyAngle { get; protected set; } // Angle variance for projectile
    public float FireRate { get; protected set; } // Seconds between shots
    public float ProjectileSpeed { get; protected set; } // Tiles
    public ushort Damage { get; protected set; } // Projectile damage
    public TextureID ProjectileTexture { get; protected set; } // Texture for the projectile
    public ItemRef? Ammo { get; protected set; }

    public RangedWeapon(ItemType itemType, int amount, string? customName = null) : base(itemType, amount, customName)
    {
        // Child classes should set FireRate, ProjectileSpeed, Damage and ProjectileTexture in their constructors
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
            float random = RandomManager.RandomFloat() * 2f - 1f;
            float offset = MathF.Sign(random) * random * random * AccuracyAngle;
            Projectile projectile = new(gameManager, 0, CameraManager.CameraDest, (float)Math.Atan2(dir.Y, dir.X) + offset, ProjectileTexture, Damage, ProjectileSpeed);
            
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

