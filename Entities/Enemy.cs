using Migs.MPath.Core.Data;
using SharpDX.Direct2D1.Effects;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Quest.Entities;
public class Enemy : IEntity
{
    public ushort UID { get; }
    public bool IsAlive => Health >= 0;
    public string Name { get; protected set; }
    public int Health { get; protected set; }
    public int Damage { get; protected set; }
    public float AttackSpeed { get; protected set; } // Attacks per second
    public float Defense { get; protected set; } // If damage <= Defense, damage /= 2
    public int Speed { get; protected set; } // Pixels per second
    public int ProjectileSpeed { get; protected set; } // Pixels per second
    public int ViewRange { get; protected set; } // Pixels
    public int AttackRange { get; protected set; } // Pixels
    public TextureID Texture { get; protected set; }
    public TextureID ProjectileTexture { get; protected set; }
    public Vector2 Position { get; protected set; }
    public float Scale { get; protected set; }
    public RectangleF Bounds => new(Position, Size);
    public Vector2 FootPosition => Position + new Vector2(Size.X / 2, Size.Y);
    public Point Size { get; set; }
    protected List<Point>? Path { get; set; }
    protected PlayerManager? PlayerManager { get; set; }
    public Enemy(Point pos, PlayerManager? playerManager = null)
    {
        Name = GetType().Name;
        Texture = TextureID.PurpleWizard; // DEBUG TODO remove this line when all enemies have textures
        ProjectileTexture = TextureID.Fireball; // DEBUG TODO remove this line when all enemies have textures
        UID = UIDManager.Get(UIDCategory.Enemies);
        Position = pos.ToVector2();
        PlayerManager = playerManager;

        // Stats
        Health = 100;
        Damage = 25;
        AttackSpeed = 2;
        Defense = 20;
        Speed = 80;
        ProjectileSpeed = 120;
        ViewRange = 1000;
        AttackRange = 500;
        Scale = Constants.PlayerScale;

        Size = (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap).Scaled(Scale);
    }
    public virtual void Update(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game) return;

        // Check death
        if (!IsAlive)
            UIDManager.Release(UIDCategory.Enemies, UID);

        // View range
        float playerDistSq = Vector2.DistanceSquared(FootPosition, CameraManager.CameraDest);

        // Attack
        if (playerDistSq < AttackRange * AttackRange)
        {
            if (TimerManager.IsCompleteOrMissing($"EnemyAttack_{UID}"))
            {
                Vector2 dir = CameraManager.PlayerFoot.ToVector2() - Bounds.Center;
                Attack(gameManager, (float)Math.Atan2(dir.Y, dir.X));
                TimerManager.SetTimer($"EnemyAttack_{UID}", AttackSpeed, null);
                Path?.Clear();
            }
        }
        // Move
        else if (playerDistSq < ViewRange * ViewRange && playerDistSq != 0)
        {
            // Update pathfinding
            if (TimerManager.IsCompleteOrMissing($"EnemyPathfind_{UID}"))
            {
                Point from = LevelManager.TileCoord(FootPosition) - CameraManager.TopLeftTileCoord;
                Point to = CameraManager.TileCoord - CameraManager.TopLeftTileCoord;
                var path = PathfindingManager.GetPath(from, to);
                Path = path != null ? [.. path.Select(p => p.ToPoint() + CameraManager.TopLeftTileCoord)] : null;
                TimerManager.SetTimer($"EnemyPathfind_{UID}", 0.5f, null);
            }
            // Move along path
            if (Path != null && Path.Count > 0)
            {
                Vector2 move = LevelManager.WorldCoord(Path[0]) + Constants.TileHalfSize.ToVector2() - FootPosition;
                if (move.LengthSquared() <= 9)
                {
                    Position += move;
                    Path.RemoveAt(0);
                }
                else
                    Position += Vector2.Normalize(move) * Speed * GameManager.DeltaTime;
            }
        }
    }
    public virtual void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime, duration: 0.5f);
        DrawTexture(gameManager.Batch, Texture, Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, source: source, scale: Scale); //, origin: new(Size.X / Scale, Size.Y / Scale));
        
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
    public virtual void Hurt(int damage)
    {
        if (damage <= Defense) Health -= damage / 2;
        else Health -= damage;
    }
    public virtual void Attack(GameManager gameManager, float direction)
    {
        Projectile proj = new(gameManager, PlayerManager, this, ProjectileTexture, Bounds.Center, direction, Damage, ProjectileSpeed);
        gameManager.LevelManager.Level.Projectiles.Add(proj);
        SoundManager.PlaySound("Fire");
    }
    public virtual void Dispose()
    {
        UIDManager.Release(UIDCategory.Enemies, UID);
    }
}
