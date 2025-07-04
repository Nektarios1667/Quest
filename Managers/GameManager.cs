using Microsoft.Xna.Framework.Content;

namespace Quest.Managers;
public class GameManager
{
    public LevelManager LevelManager { get; private set; }
    public UIManager UIManager { get; private set; }
    public float DeltaTime { get; private set; }
    public float GameTime { get; private set; } = 0f;
    public float TotalTime { get; private set; } = 0f;
    public float DayTime { get; set; } = 0f;
    public SpriteBatch Batch { get; private set; }

    public Inventory Inventory { get; set; }

    public GameManager(ContentManager content, SpriteBatch batch, Inventory inventory, LevelManager level, UIManager ui)
    {
        Batch = batch;
        Inventory = inventory;
        LevelManager = level;
        UIManager = ui;
        DeltaTime = 0;

        // Load sounds
        SoundManager.LoadSound(content, "Footstep", "Sounds/Effects/Footstep");
        SoundManager.LoadSound(content, "Trinkets", "Sounds/Effects/Trinkets");
        SoundManager.LoadSound(content, "Click", "Sounds/Effects/Click");
        SoundManager.LoadSound(content, "DoorLocked", "Sounds/Effects/DoorLocked");
        SoundManager.LoadSound(content, "DoorUnlock", "Sounds/Effects/DoorUnlock");
        SoundManager.LoadSound(content, "Spook", "Sounds/Effects/Spook");
        SoundManager.LoadSound(content, "Typing", "Sounds/Effects/Typing");
        SoundManager.LoadSound(content, "Whoosh", "Sounds/Effects/Whoosh");
        SoundManager.LoadSound(content, "Pickup", "Sounds/Effects/Pickup");
        SoundManager.LoadSound(content, "Spook", "Sounds/Effects/Spook");
        SoundManager.LoadSound(content, "Swoosh", "Sounds/Effects/Swoosh");
    }
    public void Update(float deltaTime)
    {
        DeltaTime = deltaTime;
        GameTime += deltaTime;
        DayTime += deltaTime;
        TotalTime += deltaTime;
        if (DayTime >= Constants.DayLength) DayTime = 0f;

        // Player lighting
        if (Inventory.Equipped is Light light)
            LightingManager.SetLight("PlayerLightItem", Constants.Middle + CameraManager.CameraOffset.ToPoint(), light.LightStrength, light.LightColor, 5);
        else
            LightingManager.RemoveLight("PlayerLightItem");
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
