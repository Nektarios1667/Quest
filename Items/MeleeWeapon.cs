namespace Quest.Items;

public class MeleeWeapon : Item
{
    public float FireRate { get; } // Seconds between swings
    public float Range { get; } // Tiles
    public ushort Damage { get; } // Damage dealt per hit
    public MeleeWeapon(ItemType itemType, byte amount, float firerate, float tileRange, ushort damage, string? customName = null) : base(itemType, amount, customName)
    {
        Amount = amount;
        FireRate = firerate;
        Range = tileRange;
        Damage = damage;
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing($"MeleeAttack"))
        {
            // Positioning and aiming
            Vector2 dir = InputManager.MousePosition.ToVector2() - Constants.Middle.ToVector2() - CameraManager.CameraOffset;
            // Player-owned projectiles have UID of 0
            Projectile projectile = new(gameManager, 0, CameraManager.CameraDest, (float)Math.Atan2(dir.Y, dir.X), TextureID.Slash, Damage, 0, size: Constants.TileSize.Scaled(Range));
            projectile.Position -= projectile.Size.ToVector2() / 2;
            projectile.Position += dir.NormalizedCopy() * (Range * Constants.TileSize.X / 2);

            SoundManager.PlaySound("Swoosh", pitchVariation: 0.25f);

            gameManager.LevelManager.Level.Projectiles.Add(projectile);
            TimerManager.SetTimer($"MeleeAttackDecay", 0.5f, projectile.Destroy, updateAction: (prog) => projectile.Alpha = 1 - prog * prog * prog * prog);

            TimerManager.SetTimer($"MeleeAttack", FireRate, null);

            return true;
        }
        return false;
    }
}

