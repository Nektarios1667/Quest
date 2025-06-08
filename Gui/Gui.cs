using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Quest.Gui;

public class GuiManager
{
    public List<Widget> Widgets { get; set; }
    public GuiManager()
    {
        // Initialize the GUI handler
        Widgets = [];
    }
    public void Update(float deltaTime)
    {
        // Update all widgets
        foreach (Widget widget in Widgets)
            widget.Update(deltaTime);

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
