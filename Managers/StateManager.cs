namespace Quest.Managers;

[Flags]
public enum LevelFeatures : ushort
{
    None = 0,
    Biomes = 1,
    QuillScripts = 2,
}
public enum GameState
{
    MainMenu,
    Settings,
    Credits,
    LevelSelect,
    Loading,
    Game,
    Editor,
}
public enum OverlayState
{
    None,
    Container,
    Pause,
    GUI,
    Finished,
    Death,
}
public enum Mood
{
    Calm,
    Dark,
    Epic,
}
public class StateManager
{
    // States
    public readonly Action<GameState>? OnStateChanged;
    public readonly Action<OverlayState>? OnOverlayStateChanged;
    public bool IsPlayingState => State == GameState.Game || State == GameState.Editor;
    private GameState _state = GameState.MainMenu;
    public GameState State
    {
        get => _state;
        set
        {
            PreviousState = _state;
            OnStateChanged?.Invoke(value);
            _state = value;
        }
    }
    private GameState PreviousState { get; set; } = GameState.MainMenu;
    private OverlayState _overlaystate = OverlayState.None;
    public OverlayState OverlayState
    {
        get => _overlaystate;
        set
        {
            PreviousOverlayState = _overlaystate;
            OnOverlayStateChanged?.Invoke(value);
            _overlaystate = value;
        }
    }
    private OverlayState PreviousOverlayState { get; set; } = OverlayState.None;
    public Mood Mood { get; set; } = Mood.Calm;
    public void RevertGameState()
    {
        State = PreviousState;
    }
    public void RevertOverlayState()
    {
        OverlayState = PreviousOverlayState;
    }
}
