using System.Linq;

namespace Quest.Enemies;
public class Enemy
{
    public int UID { get; }
    public bool IsAlive => Health >= 0;
    public string Name { get; protected set; }
    public int Health { get; protected set; }
    public int Damage { get; protected set; }
    public float AttackSpeed { get; protected set; } // Attacks per second
    public float Defense { get; protected set; } // If damage <= Defense, damage /= 2
    public int Speed { get; protected set; } // Pixels per second
    public int ViewRange { get; protected set; } // Pixels
    public int AttackRange { get; protected set; } // Pixels
    public TextureID Texture { get; protected set; }
    public Vector2 Location { get; protected set; }
    public RectangleF Hitbox => new(Location, tileSize);
    private Point tileSize { get; set; }
    private Point[]? path { get; set; }
    public Enemy(Point location)
    {
        Name = GetType().Name;
        //Texture = (TextureID)Enum.Parse(typeof(TextureID), GetType().Name);
        Texture = TextureID.PurpleWizard; // TODO remove this line when all enemies have textures
        UID = UIDManager.NewUID("Enemies");
        tileSize = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;
        Location = location.ToVector2();

        // Stats
        Health = 100;
        Damage = 25;
        AttackSpeed = 1;
        Defense = 20;
        Speed = 80;
        ViewRange = 900;
        AttackRange = 50;

    }
    public virtual void Update(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game) return;

        // View range
        float playerDistSq = Vector2.DistanceSquared(Location, CameraManager.CameraDest);

        // Attack
        if (TimerManager.IsCompleteOrMissing($"EnemyAttack_{UID}") && playerDistSq < AttackRange * AttackRange)
        {
            Attack();
            TimerManager.SetTimer($"EnemyAttack_{UID}", 0, null);
        }

        // Move
        else if (playerDistSq < ViewRange * ViewRange && playerDistSq != 0)
        {
            // Update pathfinding
            if (TimerManager.IsCompleteOrMissing($"EnemyPathfind_{UID}"))
            {
                path = Pathfinder.FindTilePathAStar(LevelManager.TileCoord(Location + tileSize.ToVector2() / 2), CameraManager.TileCoord);
                TimerManager.SetTimer($"EnemyPathfind_{UID}", 0.5f, null);
            }
            // Move along path
            if (path != null && path.Length > 0)
            {
                Vector2 move = LevelManager.WorldCoord(path[0]) - Location;
                if (move.LengthSquared() <= 9)
                    Location += move;
                else
                    Location += Vector2.Normalize(move) * Speed * gameManager.DeltaTime;
            }
        }
    }
    public virtual void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, gameManager.GameTime, duration: 0.5f);
        DrawTexture(gameManager.Batch, Texture, Location.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, source: source);
    }
    public virtual void Hurt(int damage)
    {
        if (damage <= Defense) Health -= damage / 2;
        else Health -= damage;
    }
    public virtual void Attack()
    {

    }
}
