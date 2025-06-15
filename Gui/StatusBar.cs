using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Gui;

public class StatusBar : Widget
{
    public int MaxValue { get; set; }
    public int CurrentValue { get; set; }
    public Point Size { get; set; }
    public Color Foreground { get; set; }
    public Color Background { get; set; }
    public StatusBar(Point location, Point size, Color foreground, Color background, int currentValue, int maxValue) : base(location)
    {
        Size = size;
        CurrentValue = currentValue;
        MaxValue = maxValue;
        Foreground = foreground;
        Background = background;
    }
    public override void Draw(SpriteBatch batch)
    {
        batch.FillRectangle(new(Location.ToVector2(), Size), Background); // Background
        batch.FillRectangle(new(Location.X, Location.Y, Size.X * CurrentValue / MaxValue, Size.Y), Foreground); // Foreground
    }
}
