namespace Quest.Interaction;

public class LinkedLabel : UIElement
{
    public Rectangle Bounds { get; private set; }
    public string OriginalText { get; private set; }
    public string DisplayedText { get; private set; }
    public Func<string>[] Links { get; private set; }
    public SpriteFont Font { get; private set; }
    public Color Foreground { get; set; }
    public Color? Background { get; set; }
    public Color? BorderColor { get; set; }
    public int BorderThickness { get; set; }
    public LinkedLabel(Point location, string text, Func<string>[] links, SpriteFont font, Color fg, Color? bg = null, Color? borderColor = null, int borderThickness = 2) : base(location)
    {
        OriginalText = text;
        DisplayedText = text;
        Links = links;
        UpdateDisplayedText();
        Font = font;
        Foreground = fg;
        Background = bg;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Bounds = new Rectangle(Location, Font.MeasureString(DisplayedText).ToPoint()).Inflated(BorderThickness, BorderThickness);
    }
    public override void Update(UserInterface ui, GameManager gameManager)
    {
        UpdateDisplayedText();
    }
    public void UpdateDisplayedText()
    {
        DisplayedText = OriginalText;
        for (int l = 0; l < Links.Length; l++)
        {
            var link = Links[l];

            // Call and replace
            string returned = link();
            DisplayedText = DisplayedText.Replace($"|{l + 1}|", returned);
        }
    }
    public override void Draw(UserInterface ui)
    {
        // Background
        if (Background.HasValue)
            ui.Batch.FillRectangle(Bounds, Background.Value);
        // Border
        if (BorderColor.HasValue)
            ui.Batch.DrawRectangle(Bounds, BorderColor.Value, BorderThickness);
        // Text
        ui.Batch.DrawString(Font, DisplayedText, Location.ToVector2(), Foreground);
    }
    public void SetText(string text)
    {
        OriginalText = text;
        UpdateDisplayedText();
        Bounds = new Rectangle(Location, Font.MeasureString(DisplayedText).ToPoint()).Inflated(BorderThickness, BorderThickness);
    }
}
