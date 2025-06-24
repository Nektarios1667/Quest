namespace Quest.Managers;
public class GameManager
{
    public LevelManager LevelManager { get; private set; }
    public UIManager UIManager { get; private set; }
    public float DeltaTime { get; private set; }
    public float TotalTime { get; private set; }
    public SpriteBatch Batch { get; private set; }

    public Inventory Inventory { get; set; }

    public GameManager(SpriteBatch batch, Inventory inventory, LevelManager level, UIManager ui)
    {
        Batch = batch;
        Inventory = inventory;
        LevelManager = level;
        UIManager = ui;
        DeltaTime = 0;
        TotalTime = 0;
    }
    public void Update(float deltaTime)
    {
        DeltaTime = deltaTime;
        TotalTime += deltaTime;
    }
    public void Respawn()
    {
        string level = LevelManager.Level.Name;
        LevelManager.UnloadLevel(level);
        LevelManager.ReadLevel(UIManager, level);
        LevelManager.LoadLevel(this, level);

        UIManager.HealthBar.CurrentValue = UIManager.HealthBar.MaxValue;
        //PlayerManager.Inventory = new(6, 4);

        CameraManager.Camera = LevelManager.Level.Spawn.ToVector2();
        CameraManager.Camera = LevelManager.Level.Spawn.ToVector2();
    }
}
