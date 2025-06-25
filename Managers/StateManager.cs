using SharpDX.Win32;

namespace Quest.Managers;
public enum GameState
{
    MainMenu,
    Settings,
    Game,
    Editor,
    Death,
}
public enum OverlayState
{
    None,
    Inventory,
    Pause,
}
public enum Mood
{
    Calm,
    Dark,
    Epic,
}
public static class StateManager
{
    public static GameState State { get; set; } = GameState.MainMenu;
    public static OverlayState OverlayState { get; set; } = OverlayState.None;
    public static Mood Mood { get; set; } = Mood.Calm;
}
