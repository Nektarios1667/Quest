using Quest.Gui;
using System.Text.RegularExpressions;

namespace Quest.Interaction;

public class ProgressBar : UIElement
{
    public float Progress { get; set; }
    public Rectangle Bounds { get; protected set; }
    public Color Background { get; protected set; }
    public Color Foreground { get; protected set; }
    public int Border { get; protected set; }
    public SpriteFont? Font { get; protected set; }
    public string Text { get; protected set; }
    public ProgressBar(Point location, Point size, ElementTheme theme, string text = "") : this(location, size, theme.Background, theme.Foreground, theme.Font, text, theme.BorderThickness) { }
    public ProgressBar(Point location, Point size, Color bg, Color fg, SpriteFont? font = null, string text = "", int border = 3) : base(location)
    {
        Bounds = new(location, size);
        Background = bg;
        Foreground = fg;
        Border = border;
        Font = font;
        Text = text;
    }
    public override void Update(UserInterface ui, GameManager gameManager)
    {
    }
    public override void Draw(UserInterface ui)
    {
        // Border
        ui.Batch.DrawRectangle(Bounds, Color.Black, Border);
        // Background
        ui.Batch.FillRectangle(Bounds, Background);
        // Foreground
        ui.Batch.FillRectangle(Bounds.Location.ToVector2(), Bounds.Size.Scaled(Progress, 1), Foreground);
        // Text
        if (Font != null && Text != "")
            ui.Batch.DrawString(Font, Text, (Bounds.Location + Bounds.Size.Scaled(0.5f) - Font.MeasureString(Text).ToPoint().Scaled(0.5f)).ToVector2(), Color.White);
    }
}
