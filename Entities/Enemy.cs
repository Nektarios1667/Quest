namespace Quest.Entities;
public class Enemy
{
    public int UID { get; }
    public bool IsAlive => Health >= 0;
    public string Name { get; private set; }
    public int Health { get; private set; }
    public int Attack { get; private set; }
    public float AttackSpeed { get; private set; } // Attacks per second
    public float Defense { get; private set; } // If damage <= Defense, damage /= 2
    public int Speed { get; private set; } // Pixels per second
    public int ViewRange { get; private set; } // Pixels
    public int AttackRange { get; private set; } // Pixels
    public TextureID Texture { get; private set; }
    public Vector2 Location { get; private set; }
    public RectangleF Hitbox => new(Location, tileSize);
    private Point tileSize { get; set; }
    public Enemy(Point location)
    {
        Name = GetType().Name;
        Texture = (TextureID)Enum.Parse(typeof(TextureID), GetType().Name);
        UID = UIDManager.NewUID("Enemies");
        tileSize = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;
        Location = location.ToVector2();
        
        // Stats
        Health = 100;
        Attack = 25;
        AttackSpeed = 1;
        Defense = 20;
        Speed = 100;
        ViewRange = 900;
        AttackRange = 50;

    }
    public virtual void Update(GameManager gameManager)
    {
        if (StateManager.State != GameState.Game) return;

        // View range
        float playerDistSq = Vector2.DistanceSquared(Location, CameraManager.CameraDest);
        // Attack
        if (TimerManager.TryIsComplete($"EnemyAttack_{UID}") && playerDistSq < AttackRange * AttackRange)
        {
            TimerManager.SetTimer($"EnemyAttack_{UID}", 0, null);
            // TODO Implement player attacks
        }
        // Move
        else if (playerDistSq < ViewRange * ViewRange && playerDistSq != 0)
            Location += Vector2.Normalize(CameraManager.CameraDest - Location) * Speed * gameManager.DeltaTime;
    }
    public virtual void Draw(GameManager gameManager)
    {
        Rectangle source = GetAnimationSource(Texture, gameManager.GameTime, duration: 0.5f);
        DrawTexture(gameManager.Batch, Texture, Location.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, source: source);
    }
    public virtual void Damage(int damage)
    {
        if (damage <= Defense) Health -= damage / 2;
        else Health -= damage;
    }
}
