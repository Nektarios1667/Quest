using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quest.Interaction;

public class ElementTheme
{
    public Color Foreground { get; set; }
    public Color Background { get; set; }
    public Color Highlight { get; set; }
    public Color BorderColor { get; set; }
    public int BorderThickness { get; set; }
    public SpriteFont Font { get; set; }
    public TextAlignment Alignment { get; set; }
    public ElementTheme(Color fg, Color bg, Color hl, Color borderColor, int borderThickness, SpriteFont font, TextAlignment alignment)
    {
        Foreground = fg;
        Background = bg;
        Highlight = hl;
        BorderColor = borderColor;
        BorderThickness = borderThickness;
        Font = font;
        Alignment = alignment;
    }
}
