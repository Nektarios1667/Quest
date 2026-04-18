using MonoGame.Extended.Input;
using System.Linq;

namespace Quest.Managers;

public enum InputAction
{
    // Game
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    FreecamUp,
    FreecamDown,
    FreecamLeft,
    FreecamRight,
    ToggleInventory,
    Hotbar1,
    Hotbar2,
    Hotbar3,
    Hotbar4,
    Hotbar5,
    Hotbar6,
    DropItem,
    Back,
    Quit,
    ToggleCollisionDebug,
    ToggleTextInfo,
    ToggleFrameInfo,
    ToggleLogInfo,
    ToggleFrameBar,
    ToggleHitboxes,
    ToggleProgramInfo,
    OpenCommandWindow,

    // Editor
    FastMove,
    EditTile,
    OpenLevel,
    NewNPC,
    DeleteNPC,
    EditNPC,
    NewLoot,
    DeleteLoot,
    NewDecal,
    DeleteDecal,
    SaveLevel,
    SaveLevelAs,
    SetSpawn,
    SetTint,
    GenerateLevel,
    ResaveLevel,
    ResaveWorld,
    NewScript,
    DeleteScript,
    SelectTileTool,
    SelectDecalTool,
    SelectBiomeTool,
    CycleToolNext,
    CycleToolPrevious,
    FloodFill,
    NewLevel,
}
public struct InputBinding(Keys? key = null, Keys[]? modifiers = null, MouseButton? mouse = null)
{
    public Keys? Key = key;
    public Keys[]? Modifiers = modifiers;
    public MouseButton? Mouse = mouse;
}
public static class InputManager
{
    static InputManager()
    {
        foreach (InputAction action in Enum.GetValues(typeof(InputAction))) {
            if (!binds.TryGetValue(action, out var _))
                Logger.Warning($"Missing bind for action '{action}'");
        }
    }
    // Bindings
    private static Dictionary<InputAction, InputBinding> binds = new()
    {
        // Game
        { InputAction.MoveUp,               new(Keys.W) },
        { InputAction.MoveDown,             new(Keys.S) },
        { InputAction.MoveLeft,             new(Keys.A) },
        { InputAction.MoveRight,            new(Keys.D) },
        { InputAction.FreecamUp,            new(Keys.W) },
        { InputAction.FreecamDown,          new(Keys.S) },
        { InputAction.FreecamLeft,          new(Keys.A) },
        { InputAction.FreecamRight,         new(Keys.D) },
        { InputAction.ToggleInventory,      new(Keys.E) },
        { InputAction.Hotbar1,              new(Keys.D1) },
        { InputAction.Hotbar2,              new(Keys.D2) },
        { InputAction.Hotbar3,              new(Keys.D3) },
        { InputAction.Hotbar4,              new(Keys.D4) },
        { InputAction.Hotbar5,              new(Keys.D5) },
        { InputAction.Hotbar6,              new(Keys.D6) },
        { InputAction.DropItem,             new(Keys.D) },
        { InputAction.Back,                 new(Keys.Escape) },
        { InputAction.Quit,                 new(Keys.Escape, [Keys.LeftAlt]) },
        { InputAction.ToggleCollisionDebug, new(Keys.F1, [Keys.LeftControl]) },
        { InputAction.ToggleTextInfo,       new(Keys.F2, [Keys.LeftControl]) },
        { InputAction.ToggleFrameInfo,      new(Keys.F3, [Keys.LeftControl]) },
        { InputAction.ToggleLogInfo,        new(Keys.F4, [Keys.LeftControl]) },
        { InputAction.ToggleFrameBar,       new(Keys.F5, [Keys.LeftControl]) },
        { InputAction.ToggleHitboxes,       new(Keys.F6, [Keys.LeftControl]) },
        { InputAction.ToggleProgramInfo,    new(Keys.F7, [Keys.LeftControl]) },
        { InputAction.OpenCommandWindow,    new(Keys.OemTilde, [Keys.LeftControl, Keys.LeftShift]) },

        // Editor
        { InputAction.FastMove,             new(Keys.LeftAlt) },
        { InputAction.EditTile,             new(Keys.M, [Keys.LeftControl]) },
        { InputAction.OpenLevel,            new(Keys.O, [Keys.LeftControl]) },
        { InputAction.NewNPC,               new(Keys.C, [Keys.LeftControl]) },
        { InputAction.DeleteNPC,            new(Keys.C, [Keys.LeftControl, Keys.LeftShift]) },
        { InputAction.EditNPC,              new(Keys.C, [Keys.LeftControl, Keys.LeftAlt]) },
        { InputAction.NewLoot,              new(Keys.L, [Keys.LeftControl]) },
        { InputAction.DeleteLoot,           new(Keys.L, [Keys.LeftControl, Keys.LeftShift]) },
        { InputAction.NewDecal,             new(Keys.D, [Keys.LeftControl]) },
        { InputAction.DeleteDecal,          new(Keys.D, [Keys.LeftControl, Keys.LeftShift]) },
        { InputAction.SaveLevel,            new(Keys.S, [Keys.LeftControl]) },
        { InputAction.SaveLevelAs,          new(Keys.S, [Keys.LeftControl, Keys.LeftShift]) },
        { InputAction.SetSpawn,             new(Keys.W, [Keys.LeftControl]) },
        { InputAction.SetTint,              new(Keys.T, [Keys.LeftControl]) },
        { InputAction.GenerateLevel,        new(Keys.G, [Keys.LeftControl]) },
        { InputAction.ResaveLevel,          new(Keys.R, [Keys.LeftControl]) },
        { InputAction.ResaveWorld,          new(Keys.R, [Keys.LeftControl, Keys.LeftShift]) },
        { InputAction.NewScript,            new(Keys.P, [Keys.LeftControl]) },
        { InputAction.DeleteScript,         new(Keys.P, [Keys.LeftControl, Keys.LeftShift]) },
        { InputAction.SelectTileTool,       new(Keys.D1) },
        { InputAction.SelectDecalTool,      new(Keys.D2) },
        { InputAction.SelectBiomeTool,      new(Keys.D3) },
        { InputAction.CycleToolNext,        new(Keys.OemCloseBrackets) },
        { InputAction.CycleToolPrevious,    new(Keys.OemOpenBrackets) },
        { InputAction.FloodFill,            new(Keys.F) },
        { InputAction.NewLevel,             new(Keys.N, [Keys.LeftControl]) },
    };

    // Devices
    public static KeyboardState KeyboardState { get; private set; }
    public static KeyboardState LastKeyboardState { get; private set; }
    public static MouseState MouseState { get; private set; }
    public static MouseState LastMouseState { get; private set; }
    public static void Update(Game game)
    {
        if (!game.IsActive) return;

        // Update input states
        LastKeyboardState = KeyboardState;
        LastMouseState = MouseState;
        KeyboardState = Keyboard.GetState();
        MouseState = Mouse.GetState();
    }
    public static Point MousePosition => MouseState.Position;
    public static Keys[] KeysDown => KeyboardState.GetPressedKeys();
    public static Keys[] LastKeysDown => LastKeyboardState.GetPressedKeys();
    public static bool KeyPressed(Keys key) => KeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
    public static bool BindDown(InputAction action, KeyboardState? keystate = null, MouseState? mousestate = null)
    {
        keystate = keystate ?? KeyboardState;
        mousestate = mousestate ?? MouseState;

        if (binds.TryGetValue(action, out var bind))
        {
            if (bind.Key.HasValue && bind.Modifiers != null && bind.Modifiers.Length > 0)
            {
                if (bind.Modifiers.Length == 1)
                    return Hotkey(bind.Modifiers[0], bind.Key.Value, keystate.Value);
                else if (bind.Modifiers.Length == 2)
                    return Hotkey(bind.Modifiers[0], bind.Modifiers[1], bind.Key.Value, keystate.Value);
            } else if (bind.Key.HasValue)
            {
                return keystate.Value.IsKeyDown(bind.Key.Value);
            } else if (bind.Mouse.HasValue)
            {
                return IsMouseDown(bind.Mouse.Value, mousestate);
            }
        }
        return false;
    }
    public static bool BindPressed(InputAction action)
    {
        return BindDown(action) && !BindDown(action, keystate: LastKeyboardState, mousestate: LastMouseState);
    }
    public static bool IsMouseDown(MouseButton button, MouseState? mousestate = null)
    {
        mousestate = mousestate ?? MouseState;

        return button switch
        {
            MouseButton.Left => mousestate.Value.LeftButton == ButtonState.Pressed,
            MouseButton.Right => mousestate.Value.RightButton == ButtonState.Pressed,
            MouseButton.Middle => mousestate.Value.MiddleButton == ButtonState.Pressed,
            _ => false
        };
    }
    private static bool Hotkey(Keys modifier, Keys key, KeyboardState keystate) => keystate.IsKeyDown(modifier) && keystate.IsKeyDown(key);
    private static bool Hotkey(Keys modifier1, Keys modifier2, Keys key, KeyboardState keystate) => keystate.IsKeyDown(modifier1) && keystate.IsKeyDown(modifier2) && keystate.IsKeyDown(key);
    public static bool LMouseClicked => MouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released;
    public static bool RMouseClicked => MouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton == ButtonState.Released;
    public static bool MMouseClicked => MouseState.MiddleButton == ButtonState.Pressed && LastMouseState.MiddleButton == ButtonState.Released;
    public static bool LMouseReleased => MouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton == ButtonState.Pressed;
    public static bool RMouseReleased => MouseState.RightButton == ButtonState.Released && LastMouseState.RightButton == ButtonState.Pressed;
    public static bool MMouseReleased => MouseState.MiddleButton == ButtonState.Released && LastMouseState.MiddleButton == ButtonState.Pressed;
    public static bool LMouseDown => MouseState.LeftButton == ButtonState.Pressed;
    public static bool RMouseDown => MouseState.RightButton == ButtonState.Pressed;
    public static bool MMouseDown => MouseState.MiddleButton == ButtonState.Pressed;
    public static int ScrollWheelValue => MouseState.ScrollWheelValue;
    public static int ScrollWheelChange => MouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue;
    public static bool ScrolledUp => ScrollWheelChange > 0;
    public static bool ScrolledDown => ScrollWheelChange < 0;
}