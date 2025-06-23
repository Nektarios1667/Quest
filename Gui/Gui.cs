namespace Quest.Gui;

public class Gui
{
    public List<Widget> Widgets { get; set; }
    public Gui()
    {
        // Initialize the GUI handler
        Widgets = [];
    }
    public void Update(GameManager gameManager)
    {
        // Update all widgets
        foreach (Widget widget in Widgets)
            widget.Update(gameManager.DeltaTime);

        // Remove expired widgets
        for (int w = 0; w < Widgets.Count; w++)
        {
            if (Widgets[w].Expired)
            {
                Widgets.RemoveAt(w);
                w--;
            }
        }
    }
    public void Draw(SpriteBatch batch)
    {
        // Draw all widgets
        foreach (Widget widget in Widgets)
        {
            if (widget.IsVisible)
                widget.Draw(batch);
        }
    }
}
