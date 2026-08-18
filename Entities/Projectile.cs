using System.Reflection.Metadata.Ecma335;

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
    public ushort Damage { get; private set; }
    public float Speed { get; private set; } // Tiles
    public bool IsAlive { get; private set; } = true;
    public Projectile(GameManager gameManager, ushort ownerUID, Vector2 position, float direction, TextureID tex, ushort damage, float speed, Point? size = null)
    {
        OwnerUID = ownerUID;
        Position = position;
        Direction = direction;
        Texture = tex;
        Damage = damage;
        Speed = speed;
        Size = size ?? (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap).Scaled(Constants.ProjectileScale);
        UID = UIDManager.Get(UIDCategory.Projectiles);

        // Update collision 60/s
        TimerManager.SetTimer($"ProjectileCollision_{UID}", 0.017f, () => UpdateCollision(gameManager), int.MaxValue);
    }
    public void Update(GameManager gameManager)
    {
        if (gameManager.StateManager.State != GameState.Game) return;

        // Move
        Position += new Vector2(MathF.Cos(Direction), MathF.Sin(Direction)) * Speed * Constants.TileSize.ToVector2() * GameManager.DeltaTime;
    }
    public void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime, duration: 0.1f);
        Vector2 texMiddle = Size.ToVector2() / Constants.ProjectileScale / 2; // Since the origin is the center (for rotation), we need to offset the position by half the size of the texture (times the scale)
        DrawTexture(gameManager.Batch, Texture, CameraManager.WorldToScreen(Position.ToPoint()) + (texMiddle * Constants.ProjectileScale).ToPoint(), source: source, origin: texMiddle, rotation: Direction, scale: new(Constants.ProjectileScale));
        // Debug
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
    public void Destroy()
    {
        // Cleanup
        UIDManager.Release(UIDCategory.Projectiles, UID);
        IsAlive = false;
        TimerManager.TryRemove($"ProjectileCollision_{UID}");
    }

    private void UpdateCollision(GameManager gameManager)
    {
        // Check collisions with walls
        Point tileCoord = CameraManager.WorldToTile(Position.ToPoint() + Size.Scaled(0.5f));
        Tile? tile = gameManager.LevelManager.GetTile(tileCoord);
        // Either OOB, or non-walkable wall
        if (tile == null || (tile.IsWall && !tile.IsWalkable))
        {
            Destroy();
            tile?.OnProjectileCollide(gameManager, this);
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
