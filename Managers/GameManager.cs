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
    public float DayTime { get; set; } = 0f;
    public SpriteBatch Batch { get; private set; }
    public SpriteBatch MinimapBatch { get; private set; }
    public Effect? GradingEffect { get; private set; }

    public GameManager(ContentManager content, SpriteBatch batch, LevelManager level, OverlayManager overlay, Effect? gradingEffect)
    {
        GradingEffect = gradingEffect;
        Batch = batch;
        MinimapBatch = new SpriteBatch(batch.GraphicsDevice);
        LevelManager = level;
        OverlayManager = overlay;
    }
    public void Update(float deltaTime)
    {
        TotalTime += deltaTime;

        // Escape button
        if (InputManager.BindPressed(InputAction.Back))
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
        if (StateManager.OverlayState != OverlayState.Pause && StateManager.OverlayState != OverlayState.Death)
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
    public void Respawn(PlayerManager playerManager)
    {
        StateManager.ReadGameState(this, playerManager, StateManager.CurrentSave.ToString());
        StateManager.OverlayState = OverlayState.None;
    }
}
