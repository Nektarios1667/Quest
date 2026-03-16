using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public enum ButtonState
{
    Normal,
    Hovered,
    Pressed,
}
public class Button : UIElement
{
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
    private Point textPosition { get; set; }
    public Button(Point location, Point size, string text, SpriteFont font, Color fg, Color bg, Color hl, Color? borderColor = null, int borderThickness = 2) : base(location)
    {
        Size = size;
        Text = text;
        Font = font;
        Foreground = fg;
        Background = bg;
        Highlight = hl;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Bounds = new Rectangle(Location, Size).Inflated(BorderThickness, BorderThickness);
        textPosition = Location + Size.Scaled(0.5f) - Font.MeasureString(Text).ToPoint().Scaled(0.5f);
    }
    public override void Update(UserInterface ui)
    {
        if (Bounds.Contains(InputManager.MousePosition))
        {
            if (InputManager.LMouseClicked)
            {
                State = ButtonState.Pressed;
                SoundManager.PlaySound("Click");
                Clicked?.Invoke();
            }
            else if (InputManager.LMouseDown)
                State = ButtonState.Pressed;
            else
                State = ButtonState.Hovered;
        }
        else
            State = ButtonState.Normal;
    }
    public override void Draw(UserInterface ui)
    {
        // Background
        ui.Batch.FillRectangle(Bounds, State == ButtonState.Normal ? Background : Highlight);
        // Border
        if (BorderColor.HasValue)
            ui.Batch.DrawRectangle(Bounds, BorderColor.Value, BorderThickness);
        // Text
        ui.Batch.DrawString(Font, Text, textPosition.ToVector2(), Foreground);
    }
    public void SetText(string text)
    {
        Text = text;
        updateBounds();
    }
    private void updateBounds()
    {
        Bounds = new Rectangle(Location, Size).Inflated(BorderThickness, BorderThickness);
        textPosition = Location + Size.Scaled(0.5f) - Font.MeasureString(Text).ToPoint().Scaled(0.5f);
    }
}
