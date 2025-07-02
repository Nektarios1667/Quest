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
        FillRectangle(batch, new(Location, Size), Background); // Background
        FillRectangle(batch, new(Location.X, Location.Y, Size.X * CurrentValue / MaxValue, Size.Y), Foreground); // Foreground
    }
}
