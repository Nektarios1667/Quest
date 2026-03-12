using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public class TextInput : UIElement
{
    public const string AllChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~ ";
    public const string AlphaNumUnderChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    public const string AlphaNumChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public const string AlphaChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string DigitChars = "0123456789";
    public const string NumChars = "0123456789.,";
    
    public event Action? Clicked;
    public Point Size { get; private set; }
    public Rectangle Bounds { get; private set; }
    public string Text { get; private set; }
    public SpriteFont Font { get; private set; }
    public Color Foreground { get; set; }
    public Color Background { get; set; }
    public Color Highlight { get; set; }
    public Color? BorderColor { get; set; }
    public int BorderThickness { get; set; }
    public ButtonState State { get; private set; } = ButtonState.Normal;
    public string AllowedChars { get; set; } = AllChars;
    public TextInput(Point location, Point size, SpriteFont font, Color fg, Color bg, Color hl, Color? borderColor = null, int borderThickness = 2) : base(location)
    {
        Size = size;
        Text = "";
        Font = font;
        Foreground = fg;
        Background = bg;
        Highlight = hl;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Bounds = new Rectangle(Location, Size).Inflated(BorderThickness, BorderThickness);
    }
    public override void Update(UserInterface ui)
    {
        // State
        if (InputManager.LMouseClicked)
        {
            if (Bounds.Contains(InputManager.MousePosition))
            {
                State = ButtonState.Pressed;
                Clicked?.Invoke();
            } else
                State = ButtonState.Normal;
        }

        if (State != ButtonState.Pressed) return;

        // Typing
        foreach (Keys key in InputManager.KeysDown)
        {
            if (InputManager.LastKeysDown.Contains(key)) continue;

            string character = KeyToString(key, InputManager.AnyKeyDown(Keys.LeftShift, Keys.RightShift));
            if (character == "\b" && Text.Length > 0)
                Text = Text[..^1];
            else if (character != "\b")
                Text += character;
        }

        // Check text
        CheckText();
    }
    public override void Draw(UserInterface ui)
    {
        // Background
        ui.Batch.FillRectangle(Bounds, State == ButtonState.Normal ? Background : Highlight);
        // Border
        if (BorderColor.HasValue)
            ui.Batch.DrawRectangle(Bounds, BorderColor.Value, BorderThickness);
        // Text
        Vector2 textPos = Location.ToVector2() + new Vector2(BorderThickness + 3, BorderThickness + 1);
        ui.Batch.DrawString(Font, Text, textPos, Foreground);
        // Cursor
        if (State == ButtonState.Pressed && GameManager.GameTime % 1 < 0.5f) {
            if (Text == "")
                ui.Batch.DrawLine(textPos, textPos + new Vector2(0, Font.MeasureString("|").Y), Foreground, 3);
            else
                ui.Batch.DrawLine(textPos + new Vector2(Font.MeasureString(Text).X, 0), textPos + Font.MeasureString(Text), Foreground, 3);
        }

    }
    public void SetText(string text)
    {
        Text = text;
        CheckText();
    }
    public void CheckText()
    {
        // Check allowed letters
        StringBuilder sb = new();
        foreach (char character in Text)
            if (AllowedChars.Contains(character))
                sb.Append(character);
        Text = sb.ToString();

        // Check length
        while (Font.MeasureString(Text).X > Size.X - BorderThickness * 2)
            Text = Text[..^1];
    }
    public static string KeyToString(Keys key, bool shift)
    {
        if (key == Keys.Back) return "\b";
        else if (key == Keys.OemTilde) return shift ? "~" : "`";
        else if (key == Keys.OemMinus) return shift ? "_" : "-";
        else if (key == Keys.OemPlus) return shift ? "+" : "=";
        else if (key == Keys.OemOpenBrackets) return shift ? "{" : "[";
        else if (key == Keys.OemCloseBrackets) return shift ? "}" : "]";
        else if (key == Keys.OemPipe) return shift ? "|" : "\\";
        else if (key == Keys.OemBackslash) return shift ? "|" : "\\";
        else if (key == Keys.OemSemicolon) return shift ? ":" : ";";
        else if (key == Keys.OemQuotes) return shift ? "\"" : "'";
        else if (key == Keys.OemComma) return shift ? "<" : ",";
        else if (key == Keys.OemPeriod) return shift ? ">" : ".";
        else if (key == Keys.OemQuestion) return shift ? "?" : "/";
        else if (key == Keys.Space) return " ";
        else if (key == Keys.Enter) return "\x0D";
        else if (key == Keys.D1) return shift ? "!" : "1";
        else if (key == Keys.D2) return shift ? "@" : "2";
        else if (key == Keys.D3) return shift ? "#" : "3";
        else if (key == Keys.D4) return shift ? "$" : "4";
        else if (key == Keys.D5) return shift ? "%" : "5";
        else if (key == Keys.D6) return shift ? "^" : "6";
        else if (key == Keys.D7) return shift ? "&" : "7";
        else if (key == Keys.D8) return shift ? "*" : "8";
        else if (key == Keys.D9) return shift ? "(" : "9";
        else if (key == Keys.D0) return shift ? ")" : "0";
        else if (key.ToString().Length > 1) return "";
        else
            return shift ? key.ToString().ToUpper() : key.ToString().ToLower();
    }
}
