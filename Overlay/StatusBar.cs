namespace Quest.Gui;
public enum StatusTextType
{
    None,
    Current,
    Max,
    Fraction,
    Decimal,
    Percentage,
}
public class StatusBar : Widget
{
    public int MaxValue { get; set; }
    public int CurrentValue { get; set; }
    public Point Size { get; set; }
    public Color Foreground { get; set; }
    public Color Background { get; set; }
    public StatusTextType TextType { get; set; }
    public SpriteFont Font { get; set; }
    public StatusBar(Point location, Point size, Color foreground, Color background, int currentValue, int maxValue, StatusTextType textType = default, SpriteFont font = null) : base(location)
    {
        Size = size;
        CurrentValue = currentValue;
        MaxValue = maxValue;
        Foreground = foreground;
        Background = background;
        TextType = textType;
        Font = font ?? PixelOperatorSmall;
    }
    public override void Draw(SpriteBatch batch)
    {
        FillRectangle(batch, new(Position, Size), Background); // Background
        FillRectangle(batch, new(Position.X, Position.Y, (int)(Size.X * (float)CurrentValue / MaxValue), Size.Y), Foreground); // Foreground
        // Text
        string text = TextType switch
        {
            StatusTextType.Current => $"{CurrentValue}",
            StatusTextType.Max => $"{MaxValue}",
            StatusTextType.Fraction => $"{CurrentValue}/{MaxValue}",
            StatusTextType.Decimal => $"{(float)CurrentValue / MaxValue:0.00}",
            StatusTextType.Percentage => $"{(float)CurrentValue / MaxValue * 100:0}%",
            _ => "",
        };
        batch.DrawString(Font, text, (Position + Size.Scaled(0.5f) - Font.MeasureString(text).ToPoint().Scaled(0.5f)).ToVector2(), Color.White);
    }
}
