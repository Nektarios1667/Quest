using Microsoft.Xna.Framework.Content;

namespace Quest.Managers;
public class GameManager
{
    // Static times
    public static float DeltaTime { get; private set; } = 0f;
    public static float GameTime { get; set; } = 0f;
    public static float TotalTime { get; private set; } = 0f;

    public LevelManager LevelManager { get; private set; }
    public OverlayManager OverlayManager { get; private set; }
    public MenuManager MenuManager { get; private set; }
    public float DayTime { get; set; } = 0f;
    public SpriteBatch Batch { get; private set; }
    public SpriteBatch MinimapBatch { get; private set; }

    public GameManager(ContentManager content, SpriteBatch batch, LevelManager level, OverlayManager overlay)
    {
        Batch = batch;
        MinimapBatch = new SpriteBatch(batch.GraphicsDevice);
        LevelManager = level;
        OverlayManager = overlay;

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

        // Escape button
        if (InputManager.KeyPressed(Keys.Escape))
        {
            // Pause/unpause
            if (StateManager.State == GameState.Game)
            {
                if (StateManager.OverlayState == OverlayState.None)
                    StateManager.OverlayState = OverlayState.Pause;
                else if (StateManager.OverlayState == OverlayState.Pause)
                    StateManager.OverlayState = OverlayState.None;
            }
        }

        // Time
        if (StateManager.OverlayState != OverlayState.Pause)
        {
            DeltaTime = deltaTime;
            GameTime += deltaTime;
            if (StateManager.State == GameState.Game)
                DayTime += deltaTime;
            if (DayTime >= Constants.DayLength) DayTime = 0f;
        }
        else
            DeltaTime = 0f;
    }
    public void Respawn()
    {
        string level = LevelManager.Level.Name;
        LevelManager.ReadLevel(this, level, reload: true);
        LevelManager.LoadLevel(this, level);

        OverlayManager.HealthBar.CurrentValue = OverlayManager.HealthBar.MaxValue;
        //PlayerManager.Inventory = new(6, 4);

        CameraManager.Camera = LevelManager.Level.Spawn.ToVector2();
        CameraManager.Camera = LevelManager.Level.Spawn.ToVector2();
    }
}
