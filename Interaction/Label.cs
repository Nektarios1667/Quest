namespace Quest.Interaction;

public enum TextAlignment
{
    Left,
    Center,
    Right
}

public class Label : UIElement
{
    public Rectangle Bounds { get; private set; }
    public string Text { get; private set; }
    public SpriteFont Font { get; private set; }
    public Color Foreground { get; set; }
    public Color? Background { get; set; }
    public Color? BorderColor { get; set; }
    public int BorderThickness { get; set; }
    public TextAlignment Alignment { get; set; }
    public Label(Point location, string text, SpriteFont font, Color fg, Color? bg = null, Color? borderColor = null, int borderThickness = 2, TextAlignment alignment = TextAlignment.Left) : base(location)
    {
        Text = text;
        Font = font;
        Foreground = fg;
        Background = bg;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Alignment = alignment;
        Bounds = new Rectangle(Location, Font.MeasureString(Text).ToPoint()).Inflated(BorderThickness, BorderThickness);
    }
    public override void Update(UserInterface ui, GameManager gameManager) { }
    public override void Draw(UserInterface ui)
    {
        // Background
        if (Background.HasValue)
            ui.Batch.FillRectangle(Bounds, Background.Value);
        // Border
        if (BorderColor.HasValue)
            ui.Batch.DrawRectangle(Bounds, BorderColor.Value, BorderThickness);
        // Text
        var textPosition = Location.ToVector2();
        switch (Alignment)
        {
            case TextAlignment.Left:
                break;
            case TextAlignment.Center:
                textPosition.X -= Font.MeasureString(Text).X / 2;
                break;
            case TextAlignment.Right:
                textPosition.X -= Font.MeasureString(Text).X;
                break;
        }
        ui.Batch.DrawString(Font, Text, textPosition, Foreground);
    }
    public void SetText(string text)
    {
        Text = text;
        Bounds = new Rectangle(Location, Font.MeasureString(Text).ToPoint()).Inflated(BorderThickness, BorderThickness);
    }
}
