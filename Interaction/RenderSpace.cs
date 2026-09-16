namespace Quest.Interaction;

public class RenderSpace : UIElement
{
    public Point Size { get; private set; }
    public Func<RenderTarget2D?, RenderTarget2D?> Renderer { get; private set; }
    public RenderTarget2D? CurrentRender { get; private set; }
    public Rectangle Rect { get; private set; }
    public Color? Background { get; private set; }
    public Color? BorderColor { get; private set; }
    public int BorderThickness { get; private set; }
    public RenderSpace(Point location, Point size, Func<RenderTarget2D?, RenderTarget2D?> renderer, Color? bg = null, Color? borderColor = null, int borderThickness = 0) : base(location)
    {
        Size = size;
        Renderer = renderer;
        Background = bg;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Rect = new(location, size);
    }
    public override void Update(UserInterface ui, GameManager gameManager)
    {

    }
    public override void Draw(UserInterface ui)
    {
        if (!IsVisible) return;

        // Get render
        CurrentRender = Renderer(null); // A null render means that the renderer func will have to rebuild the render
        if (CurrentRender == null) return;

        // Draw
        // Bg
        if (Background != null)
            ui.Batch.FillRectangle(Rect, Background.Value);
        // Render
        ui.Batch.Draw(CurrentRender, Location.ToVector2(), Color.White);
        // Border
        if (BorderColor != null)
            ui.Batch.DrawRectangle(Rect, BorderColor.Value, BorderThickness);
    }
    public void MarkReRenderFlag() => CurrentRender = null;
}
