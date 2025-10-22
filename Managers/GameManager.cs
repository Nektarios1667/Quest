using Microsoft.Xna.Framework.Content;

namespace Quest.Managers;
public class GameManager
{
    public LevelManager LevelManager { get; private set; }
    public OverlayManager UIManager { get; private set; }
    public float DeltaTime { get; private set; } = 0f;
    public float GameTime { get; private set; } = 0f;
    public float TotalTime { get; private set; } = 0f;
    public float DayTime { get; set; } = 0f;
    public SpriteBatch Batch { get; private set; }

    public GameManager(ContentManager content, SpriteBatch batch, LevelManager level, OverlayManager ui)
    {
        Batch = batch;
        LevelManager = level;
        UIManager = ui;

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
        SoundManager.LoadSound(content, "Swoosh", "Sounds/Effects/Swoosh");
    }
    public void Update(float deltaTime)
    {
        TotalTime += deltaTime;

        // Pause
        if (InputManager.KeyPressed(Keys.Escape))
        {
            if (StateManager.OverlayState == OverlayState.None)
                StateManager.OverlayState = OverlayState.Pause;
            else if (StateManager.OverlayState == OverlayState.Pause)
                StateManager.OverlayState = OverlayState.None;
        }

        // Time
        if (StateManager.OverlayState != OverlayState.Pause)
        {
            DeltaTime = deltaTime;
            GameTime += deltaTime;
            if (StateManager.State == GameState.Game)
                DayTime += deltaTime;
            if (DayTime >= Constants.DayLength) DayTime = 0f;
        } else
            DeltaTime = 0f;
    }
    public void Respawn()
    {
        string level = LevelManager.Level.Name;
        LevelManager.ReadLevel(UIManager, level, reload:true);
        LevelManager.LoadLevel(this, level);

        UIManager.HealthBar.CurrentValue = UIManager.HealthBar.MaxValue;
        //PlayerManager.Inventory = new(6, 4);

        CameraManager.Camera = LevelManager.Level.Spawn.ToVector2();
        CameraManager.Camera = LevelManager.Level.Spawn.ToVector2();
    }
}
