using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;

namespace Quest.Managers;

public class GameManager
{
    // Static times
    public static ulong FrameCount { get; private set; } = 0;
    public static float DeltaTime { get; private set; } = 0f;
    public static float GameTime { get; set; } = 0f;
    public static float TotalTime { get; private set; } = 0f;

    public LevelManager LevelManager { get; private set; }
    public OverlayManager OverlayManager { get; private set; }
    public WeatherManager WeatherManager { get; private set; }
    public StateManager StateManager { get; private set; } = new StateManager();
    public float DayTime { get; set; } = 0f;
    public SpriteBatch Batch { get; private set; }
    public SpriteBatch MinimapBatch { get; private set; }
    public Effect? GradingEffect { get; private set; }

    public GameManager(SpriteBatch batch, LevelManager level, OverlayManager? overlay, WeatherManager? weatherManager, Effect? gradingEffect)
    {
        if (overlay == null && StateManager.State == GameState.Game)
            Logger.Error("No OverlayManager object for the GameManager!");

        GradingEffect = gradingEffect;
        Batch = batch;
        MinimapBatch = batch != null ? new SpriteBatch(batch.GraphicsDevice) : null!;
        LevelManager = level;
        OverlayManager = overlay!; // Allow null OverlayManager for level editor. Not using nullable OverlayManager property just for convenience.
        WeatherManager = weatherManager!; // Allow null WeatherManager for level editor. Not using nullable WeatherManager property just for convenience.
    }
    public void Update(float deltaTime)
    {
        FrameCount++;
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
                DayTime += deltaTime * (InputManager.KeyDown(Keys.J) ? 10 : 1);
            if (DayTime >= Constants.DayLength) DayTime = 0f;
        }
        else
            DeltaTime = 0f;
        DebugManager.DeltaHistory[FrameCount % (ulong)DebugManager.DeltaHistory.Length] = deltaTime;
    }
    private bool respawning = false;
    public async Task Respawn(PlayerManager playerManager)
    {
        if (respawning) return;
        respawning = true;

        try
        {
            StateManager.OverlayState = OverlayState.None;
            StatusManager.ClearAllStatusEffects(this, playerManager);

            await SaveManager.ReadGameState(this, playerManager, SaveManager.CurrentSave);

            TimerManager.TryRemove("ScreenFadeOut");
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }

        respawning = false;
    }
}
