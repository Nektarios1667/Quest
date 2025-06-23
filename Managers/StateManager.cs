namespace Quest.Managers;
public enum GameState
{
    MainMenu,
    Settings,
    Game,
    Death,
}
public static class StateManager
{
    public static GameState State { get; set; } = GameState.MainMenu;
}
