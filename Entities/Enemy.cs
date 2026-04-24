using Migs.MPath.Core.Data;
using Quest.Gui;
using SharpDX.Direct2D1.Effects;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Quest.Entities;
public class Enemy : IEntity, IProjectileOwner
{
    public ushort UID { get; set; }
    public bool IsAlive => Health > 0;
    public int Health { get; set; }
    public ushort Damage { get; set; }
    public float AttackSpeed { get; set; } // Attacks per second
    public ushort Defense { get; set; } // If damage <= Defense, damage /= 2
    public ushort Speed { get; set; } // Pixels per second
    public ushort ProjectileSpeed { get; set; } // Pixels per second
    public ushort ViewRange { get; set; } // Pixels
    public ushort AttackRange { get; set; } // Pixels
    public TextureID Texture { get; set; }
    public TextureID ProjectileTexture { get; set; }
    public Vector2 Position { get; set; }
    public float Scale { get; protected set; }
    public RectangleF Bounds => new(Position, Size);
    public Vector2 FootPosition => Position + new Vector2(Size.X / 2, Size.Y);
    public Point Size { get; set; }
    public StatusBar HealthBar { get; protected set; }
    protected List<Point>? Path { get; set; }
    public Enemy(
        Vector2 pos,
        ushort health,
        ushort damage,
        float attackSpeed,
        ushort defense,
        ushort speed,
        ushort projectileSpeed,
        ushort viewRange,
        ushort attackRange,
        TextureID texture,
        TextureID projectileTexture,
        ushort? uid = null)
    {
        Texture = texture;
        ProjectileTexture = projectileTexture;
        UID = uid ?? UIDManager.Get(UIDCategory.Enemies);
        Position = pos;

        // Stats
        Health = health;
        Damage = damage;
        AttackSpeed = attackSpeed;
        Defense = defense;
        Speed = speed;
        ProjectileSpeed = projectileSpeed;
        ViewRange = viewRange;
        AttackRange = attackRange;
        Scale = Constants.PlayerScale;

        TimerManager.SetTimer($"EnemyAttack_{UID}", AttackSpeed, null);
        Size = (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap).Scaled(Scale);

        HealthBar = new(Point.Zero, new(Size.X, 10), Color.Green * 0.7f, Color.Red * 0.7f, Health, Health);
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

        // Healthbar
        HealthBar.Position = Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle + new Point(0, Size.Y + 10);
        HealthBar.CurrentValue = Health;
        HealthBar.Update(GameManager.DeltaTime);
    }
    public virtual void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime, duration: 0.5f);
        DrawTexture(gameManager.Batch, Texture, Position.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, source: source, scale: Scale); //, origin: new(Size.X / Scale, Size.Y / Scale));
        
        DebugManager.DrawHitbox(gameManager.Batch, this);
        HealthBar.Draw(gameManager.Batch);
    }
    public virtual void Hurt(ushort damage)
    {
        if (damage <= Defense) Health -= damage / 2;
        else Health -= damage;
    }
    public virtual void Attack(GameManager gameManager, float direction)
    {
        Projectile proj = new(gameManager, this, Bounds.Center, direction);
        gameManager.LevelManager.Level.Projectiles.Add(proj);
        SoundManager.PlaySound("Fire");
    }
    public virtual void Dispose()
    {
        UIDManager.Release(UIDCategory.Enemies, UID);
    }
}
