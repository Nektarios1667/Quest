using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Entities;
public class Projectile : IEntity
{
    public IEntity? Owner { get; protected set; }
    public Vector2 Position { get; protected set; }
    public float Direction { get; protected set; } // Radians, 0 = right, positive counterclockwise
    public Point Size { get; protected set; }
    public RectangleF Bounds => new(Position, Size);
    public ushort UID { get; }
    public TextureID Texture { get; protected set; }
    public int Damage { get; protected set; }
    public int Speed { get; protected set; }
    public Color? Tint { get; protected set; }
    public bool IsAlive { get; protected set; } = true;
    public Projectile(GameManager gameManager, PlayerManager? playerManager, IEntity? owner, TextureID texture, Vector2 position, float direction, int damage, int speed, Color? tint = null)
    {
        Position = position;
        Direction = direction;
        Damage = damage;
        Speed = speed;
        Texture = texture;
        Size = (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap).Scaled(Constants.ProjectileScale);
        UID = UIDManager.Get(UIDCategory.Projectiles);
        Tint = tint ?? Color.White;

        // Update collision 60/s
        TimerManager.SetTimer($"ProjectileCollision_{UID}", 0.017f, () => UpdateCollision(gameManager, playerManager), int.MaxValue);
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
        DrawTexture(gameManager.Batch, Texture, Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle + (texMiddle * Constants.ProjectileScale).ToPoint(), source: source, color: Tint, origin: texMiddle, rotation: Direction, scale: Constants.ProjectileScale);
        // Debug
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
    public void Destroy()
    {
        // Cleanup
        UIDManager.Release(UIDCategory.Projectiles, UID);
        IsAlive = false;
        TimerManager.Remove($"ProjectileCollision_{UID}");
    }

    private void UpdateCollision(GameManager gameManager, PlayerManager? playerManager)
    {
        // Check collisions with walls
        Point tileCoord = CameraManager.PositionToWorldCoord(Position + Size.ToVector2() / 2);
        if (gameManager.LevelManager.Level.Tiles[tileCoord.Y * Constants.MapSize.X + tileCoord.X].IsWall)
            Destroy();

        // Check collisions with enemies
        if (playerManager != null && Bounds.Intersects(playerManager.Bounds))
        {
            PlayerManager.DamagePlayer(gameManager, Damage);
            Destroy();
        }
    }
}
