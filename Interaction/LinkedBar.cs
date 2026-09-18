namespace Quest.Interaction;

public class LinkedBar : ProgressBar
{
    public Func<float> ProgressFunction { get; private set; }
    public Func<float, string>? DisplayFunc { get; private set; }
    public string DisplayedText { get; private set; }

    public LinkedBar(Point location, Point size, Func<float> progressFunc, Func<float, string>? displayFunc, ElementTheme theme) : this(location, size, progressFunc, displayFunc, theme.Font, theme.Background, theme.Foreground, theme.BorderThickness) { }
    public LinkedBar(Point location, Point size, Func<float> progressFunc, Func<float, string>? displayFunc, SpriteFont font, Color bg, Color fg, int border = 3) : base(location, size, bg, fg, font, "", border)
    {
        DisplayFunc = displayFunc;
        ProgressFunction = progressFunc;
        DisplayedText = "";
    }
    public override void Update(UserInterface ui, GameManager gameManager)
    {
        if (!IsEnabled) return;

        UpdateProgress();
        UpdateDisplayedText();
    }
    public void UpdateProgress()
    {
        Progress = ProgressFunction();
    }
    public void UpdateDisplayedText()
    {
        DisplayedText = Text = Progress.ToString();

        if (DisplayFunc == null) return;

        // Call and replace
        string returned = DisplayFunc(Progress);
        DisplayedText = returned;
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
        if (Font != null && DisplayedText != "")
            ui.Batch.DrawString(Font, DisplayedText, (Bounds.Location + Bounds.Size.Scaled(0.5f) - Font.MeasureString(DisplayedText).ToPoint().Scaled(0.5f)).ToVector2(), Color.White);
    }
}
