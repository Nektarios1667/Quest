namespace Quest.Managers;
public static class InputManager
{
    // Devices
    private static KeyboardState keyboardState;
    private static KeyboardState lastKeyboardState;
    private static MouseState mouseState;
    private static MouseState lastMouseState;
    public static void Update()
    {
        // Update input states
        lastKeyboardState = keyboardState;
        lastMouseState = mouseState;
        keyboardState = Keyboard.GetState();
        mouseState = Mouse.GetState();
    }
    public static Point MousePosition => mouseState.Position;
    public static Keys[] KeysPressed => keyboardState.GetPressedKeys();
    public static bool KeyPressed(Keys key) => keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key);
    public static bool KeyDown(Keys key) => keyboardState.IsKeyDown(key);
    public static bool AnyKeyDown(params Keys[] keys)
    {
        foreach (Keys key in keys)
            if (keyboardState.IsKeyDown(key)) return true;
        return false;
    }
    public static bool AllKeysDown(params Keys[] keys)
    {
        foreach (Keys key in keys)
            if (!keyboardState.IsKeyDown(key)) return false;
        return true;
    }
    public static bool Hotkey(Keys modifier, Keys key) => keyboardState.IsKeyDown(modifier) && KeyPressed(key);
    public static bool Hotkey(Keys modifier1, Keys modifier2, Keys key) => keyboardState.IsKeyDown(modifier1) && keyboardState.IsKeyDown(modifier2) && KeyPressed(key);
    public static bool LMouseClicked => mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released;
    public static bool RMouseClicked => mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released;
    public static bool MMouseClicked => mouseState.MiddleButton == ButtonState.Pressed && lastMouseState.MiddleButton == ButtonState.Released;
    public static bool LMouseReleased => mouseState.LeftButton == ButtonState.Released && lastMouseState.LeftButton == ButtonState.Pressed;
    public static bool RMouseReleased => mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed;
    public static bool MMouseReleased => mouseState.MiddleButton == ButtonState.Released && lastMouseState.MiddleButton == ButtonState.Pressed;
    public static bool LMouseDown => mouseState.LeftButton == ButtonState.Pressed;
    public static bool RMouseDown => mouseState.RightButton == ButtonState.Pressed;
    public static bool MMouseDown => mouseState.MiddleButton == ButtonState.Pressed;
    public static int ScrollWheelValue => mouseState.ScrollWheelValue;
    public static int ScrollWheelChange => mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;
}