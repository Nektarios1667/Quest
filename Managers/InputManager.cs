namespace Quest.Managers;
public static class InputManager
{
    // Devices
    public static KeyboardState KeyboardState { get; private set; }
    public static KeyboardState LastKeyboardState {get; private set;}
    public static MouseState MouseState {get; private set;}
    public static MouseState LastMouseState {get; private set;}
    public static void Update()
    {
        // Update input states
        LastKeyboardState = KeyboardState;
        LastMouseState = MouseState;
        KeyboardState = Keyboard.GetState();
        MouseState = Mouse.GetState();
    }
    public static Point MousePosition => MouseState.Position;
    public static Keys[] KeysPressed => KeyboardState.GetPressedKeys();
    public static bool KeyPressed(Keys key) => KeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
    public static bool KeyDown(Keys key) => KeyboardState.IsKeyDown(key);
    public static bool AnyKeyDown(params Keys[] keys)
    {
        foreach (Keys key in keys)
            if (KeyboardState.IsKeyDown(key)) return true;
        return false;
    }
    public static bool AllKeysDown(params Keys[] keys)
    {
        foreach (Keys key in keys)
            if (!KeyboardState.IsKeyDown(key)) return false;
        return true;
    }
    public static bool Hotkey(Keys modifier, Keys key) => KeyboardState.IsKeyDown(modifier) && KeyPressed(key);
    public static bool Hotkey(Keys modifier1, Keys modifier2, Keys key) => KeyboardState.IsKeyDown(modifier1) && KeyboardState.IsKeyDown(modifier2) && KeyPressed(key);
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
}