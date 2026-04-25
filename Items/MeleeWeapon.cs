namespace Quest.Items;
public class MeleeWeapon : Item
{
    public float FireRate { get; } // Seconds between swings
    public byte Range { get; } // Pixels
    public ushort Damage { get; } // Damage dealt per hit
    public MeleeWeapon(ItemType itemType, byte amount, float firerate, byte range, ushort damage, string? customName = null) : base(itemType, amount, customName)
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
            // Player-owned projectiles have UID of 0
            Projectile projectile = new Projectile(gameManager, 0, CameraManager.CameraDest, (float)Math.Atan2(dir.Y, dir.X), TextureID.Slash, Damage, 0, size: new(Range));
            projectile.Position -= projectile.Size.ToVector2() / 2;
            projectile.Position += dir.NormalizedCopy() * (Range / 2);

            gameManager.LevelManager.Level.Projectiles.Add(projectile);
            TimerManager.SetTimer($"MeleeAttackDecay_{UID}", FireRate / 2, projectile.Destroy);

            TimerManager.SetTimer($"MeleeAttack_{UID}", FireRate, null);
        }
    }
}

