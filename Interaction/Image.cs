using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;
public class Image : UIElement
{
    public Rectangle Bounds => Texture.Bounds.Inflated(BorderThickness, BorderThickness);
    public Color? BorderColor { get; set; }
    public int BorderThickness { get; set; }
    public Texture2D Texture { get; private set; }
    public Image(Point location, Texture2D tex, Color? borderColor = null, int borderThickness = 2) : base(location)
    {
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Texture = tex;
    }
    public override void Update(UserInterface ui) {}
    public override void Draw(UserInterface ui)
    {
        // Border
        if (BorderColor.HasValue)
            ui.Batch.DrawRectangle(Bounds, BorderColor.Value, BorderThickness);
        // Image
        ui.Batch.Draw(Texture, Location.ToVector2(), Color.White);
    }
    public void SetTexture(Texture2D image)
    {
        Texture = image;
    }
}
