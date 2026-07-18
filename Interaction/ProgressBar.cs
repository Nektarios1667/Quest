using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public class ProgressBar : UIElement
{
    public float Progress { get; set; }
    public Rectangle Bounds { get; protected set; }
    public Color Background { get; protected set; }
    public Color Foreground { get; protected set; }
    public int Border { get; protected set;  }
    public ProgressBar(Point location, Point size, Color bg, Color fg, int border) : base(location)
    {
        Bounds = new(location, size);
        Background = bg;
        Foreground = fg;
        Border = border;
    }
    public override void Update(UserInterface ui)
    {
    }
    public override void Draw(UserInterface ui)
    {
        // Border
        ui.Batch.DrawRectangle(Bounds, Color.Black, Border);
        // Background
        ui.Batch.FillRectangle(Bounds, Background);
        // Foreground
        ui.Batch.FillRectangle(Bounds.Location.ToVector2(), Bounds.Size.Scaled(Progress), Foreground);
    }
}
