using System.Threading.Tasks;

namespace Quest.Managers;

public class GameManager
{
    // Static times
    public static ulong FrameCount { get; private set; } = 0;
    public static float GameTime { get; set; } = 0f;
    public static float DeltaGameTime { get; set; } = 0f;
    public static float TimeScale { get; set; } = 1f;
    public static float RealTime { get; private set; } = 0f;
    public static float DeltaRealTime { get; private set; } = 0f;
    public static float DayTime { get; set; } = 0f;

    public LevelManager LevelManager { get; private set; }
    public OverlayManager OverlayManager { get; private set; }
    public WeatherManager WeatherManager { get; private set; }
    public StateManager StateManager { get; private set; } = new StateManager();
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
    public void Update(float deltaRealTime)
    {
        FrameCount++;
        DeltaRealTime = deltaRealTime;
        RealTime += deltaRealTime;

        // Time
        if (StateManager.OverlayState != OverlayState.Pause && StateManager.State == GameState.Game)
        {
            DeltaGameTime = deltaRealTime * TimeScale;
            GameTime += DeltaGameTime;
            DayTime = (DayTime + DeltaGameTime) % Constants.DayLength;
        }
        else
        {
            DeltaGameTime = 0;
        }
        DebugManager.DeltaHistory[FrameCount % (ulong)DebugManager.DeltaHistory.Length] = deltaRealTime;
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
