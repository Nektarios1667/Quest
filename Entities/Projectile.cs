using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

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
    public ushort Speed { get; private set; }
    public bool IsAlive { get; private set; } = true;
    public Projectile(GameManager gameManager, ushort ownerUID, Vector2 position, float direction, TextureID tex, ushort damage, ushort speed, Point? size = null)
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
        if (StateManager.State != GameState.Game) return;

        // Move
        Position += new Vector2(MathF.Cos(Direction), MathF.Sin(Direction)) * Speed * GameManager.DeltaTime;
    }
    public void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime, duration: 0.5f);
        Vector2 texMiddle = Size.ToVector2() / Constants.ProjectileScale / 2; // Since the origin is the center (for rotation), we need to offset the position by half the size of the texture (times the scale)
        DrawTexture(gameManager.Batch, Texture, Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle + (texMiddle * Constants.ProjectileScale).ToPoint(), source: source, origin: texMiddle, rotation: Direction, scale: Constants.ProjectileScale);
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
        Point tileCoord = CameraManager.PositionToWorldCoord(Position + Size.ToVector2() / 2);
        // Either OOB, or non-walkable wall
        if (tileCoord.X < 0 || tileCoord.X >= Constants.MapSize.X || tileCoord.Y < 0 || tileCoord.Y >= Constants.MapSize.Y ||
            (gameManager.LevelManager.Level.Tiles[tileCoord.Y * Constants.MapSize.X + tileCoord.X].IsWall &&
            !gameManager.LevelManager.Level.Tiles[tileCoord.Y * Constants.MapSize.X + tileCoord.X].IsWalkable))
            Destroy();
    }
}
