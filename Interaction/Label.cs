using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;
public class Label : UIElement
{
    public Rectangle Bounds { get; private set; }
    public string Text { get; private set; }
    public SpriteFont Font { get; private set; }
    public Color Foreground { get; set; }
    public Color? Background { get; set; }
    public Color? BorderColor { get; set; }
    public int BorderThickness { get; set; }
    public Label(Point location, string text, SpriteFont font, Color fg, Color? bg = null, Color? borderColor = null, int borderThickness = 2) : base(location)
    {
        Text = text;
        Font = font;
        Foreground = fg;
        Background = bg;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Bounds = new Rectangle(Location, Font.MeasureString(Text).ToPoint()).Inflated(BorderThickness, BorderThickness);
    }
    public override void Update(UserInterface ui) {}
    public override void Draw(UserInterface ui)
    {
        // Background
        if (Background.HasValue)
            ui.Batch.FillRectangle(Bounds, Background.Value);
        // Border
        if (BorderColor.HasValue)
            ui.Batch.DrawRectangle(Bounds, BorderColor.Value, BorderThickness);
        // Text
        ui.Batch.DrawString(Font, Text, Location.ToVector2(), Foreground);
    }
    public void SetText(string text)
    {
        Text = text;
        Bounds = new Rectangle(Location, Font.MeasureString(Text).ToPoint()).Inflated(BorderThickness, BorderThickness);
    }
}
