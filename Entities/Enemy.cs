namespace Quest.Entities;
public class Enemy
{
    public string Name { get; private set; }
    public int Health { get; private set; }
    public int Attack { get; private set; }
    public float AttackSpeed { get; private set; }
    public float AttackCooldown { get; private set; } = 0f;
    public float Defense { get; private set; }
    public int Speed { get; private set; }
    public int ViewRange { get; private set; }
    public int AttackRange { get; private set; }
    public TextureManager.TextureID Texture { get; private set; }
    public Vector2 Location { get; private set; }
    public string Mode { get; private set; }
    private Point tileSize { get; set; }
    public Enemy(string name, int health, Point location, int attack, float attackSpeed, float defense, int speed, int viewRange, int attackRange, TextureManager.TextureID texture)
    {
        Name = name;
        Health = health;
        Location = location.ToVector2();
        Attack = attack;
        AttackSpeed = attackSpeed;
        Defense = defense;
        Speed = speed;
        Texture = texture;
        ViewRange = viewRange;
        AttackRange = attackRange;
        Mode = "idle";
        tileSize = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;
    }
    public virtual void Update(GameManager gameManager)
    {
        // View range
        float playerDistSq = Vector2.DistanceSquared(Location, CameraManager.CameraDest);
        // Attack
        if (playerDistSq < AttackRange * AttackRange && AttackCooldown <= 0)
        {
            Mode = "attack";
            gameManager.PlayerManager.DamagePlayer(gameManager, Attack);
            AttackCooldown = AttackSpeed;
        }
        // Move
        else if (playerDistSq < ViewRange * ViewRange && playerDistSq != 0)
        {
            Mode = "move";
            Location += Vector2.Normalize(CameraManager.CameraDest - Location) * Speed * gameManager.DeltaTime;
        }
        else
            Mode = "idle";

        // Final
        if (AttackCooldown > 0) AttackCooldown -= gameManager.DeltaTime;
    }
    public virtual void Draw(GameManager gameManager)
    {
        Rectangle source = TextureManager.GetAnimationSource(Texture, gameManager.TotalTime, duration: 0.5f);
        TextureManager.DrawTexture(gameManager.Batch, Texture, Location.ToPoint() - CameraManager.Camera.ToPoint() + Constants.Middle, source: source);
    }
    public virtual void Damage(int damage)
    {
        if (damage <= Defense) Health -= (int)(damage * (1f - Defense / (Defense + 500)));
        else Health -= damage;
    }
}
