namespace Quest.Entities;

public class Projectile : IEntity
{
    public ushort OwnerUID { get; private set; }
    public Vector2 Position { get; set; }
    public float Direction { get; set; } // Radians, 0 = right, positive counterclockwise
    public Point Size { get; private set; }
    public RectangleF Bounds => new(Position, Size);
    public ushort UID { get; }
    public TextureID Texture { get; private set; }
    public float Alpha { get; set; } = 1;
    public ushort Damage { get; private set; }
    public float Speed { get; private set; } // Tiles
    public bool IsAlive { get; private set; } = true;
    public bool IsStuck { get; private set; } = false;
    public Projectile(GameManager gameManager, ushort ownerUID, Vector2 position, float direction, TextureID tex, ushort damage, float speed, Point? size = null)
    {
        OwnerUID = ownerUID;
        Position = position;
        Direction = direction;
        Texture = tex;
        Damage = damage;
        Speed = speed;
        Size = size ?? (TextureManager.Metadata[Texture].TileSize).Scaled(Constants.ProjectileScale);
        UID = UIDManager.Get(UIDCategory.Projectiles);

        // Update collision 20/s
        TimerManager.SetTimer($"ProjectileCollision_{UID}", 0.05f, false, () => UpdateCollision(gameManager), int.MaxValue);
    }
    public void Update(GameManager gameManager)
    {
        if (gameManager.StateManager.State != GameState.Game || IsStuck) return;

        // Move
        Position += new Vector2(MathF.Cos(Direction), MathF.Sin(Direction)) * Speed * Constants.TileSize.ToVector2() * GameManager.DeltaGameTime;
    }
    public void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime, duration: 0.1f);
        Vector2 texMiddle = Size.ToVector2() / Constants.ProjectileScale / 2; // Since the origin is the center (for rotation), we need to offset the position by half the size of the texture (times the scale)
        DrawTexture(gameManager.Batch, Texture, CameraManager.WorldToScreen(Position.ToPoint()) + (texMiddle * Constants.ProjectileScale).ToPoint(), source: source, origin: texMiddle, rotation: Direction, scale: new(Constants.ProjectileScale), color: Color.White * Alpha);
        // Debug
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
    public void Hit()
    {
        TimerManager.TryRemove($"ProjectileCollision_{UID}");
        TimerManager.SetTimer($"ProjectileDespawn_{UID}", 1f, false, Destroy, updateAction: (prog) => Alpha = 1 - prog * prog * prog);
        IsStuck = true;
    }
    public void Destroy()
    {
        // Cleanup
        UIDManager.Release(UIDCategory.Projectiles, UID);
        IsAlive = false;
    }

    private void UpdateCollision(GameManager gameManager)
    {
        // Check collisions with walls
        Point tileCoord = CameraManager.WorldToTile(Position.ToPoint() + Size.Scaled(0.5f));
        Tile? tile = gameManager.LevelManager.GetTile(tileCoord);
        // Either OOB, or non-walkable wall
        if (tile == null || (tile.IsWall && !tile.IsWalkable))
        {
            tile?.OnProjectileCollide(gameManager, this);
            Hit();
        }
    }
    public (StatusEffect effect, float duration)? GetProjectileEffect()
    {
        return Texture switch
        {
            TextureID.Fireball => (StatusEffect.Burning, 5),
            TextureID.DeleriumSpell => (StatusEffect.Delerium, 5),
            TextureID.HealingSpell => (StatusEffect.Regeneration, 5),
            TextureID.PoisonPotion => (StatusEffect.Poison, 3),
            TextureID.SlownessSpell => (StatusEffect.Slowness, 10),
            TextureID.VulnerabilitySpell => (StatusEffect.Vulnerability, 5),
            TextureID.WeaknessSpell => (StatusEffect.Weakness, 10),
            _ => null,
        };
    }
}
