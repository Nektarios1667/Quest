using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quest.Gui;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Gui
{
    public class Dialog : Widget
    {
        public GuiManager Gui { get; private set; }
        public Xna.Vector2 Dimensions { get; private set; }
        public Rectangle Rect { get; private set; }
        public Xna.Color Color { get; private set; }
        public int Border { get; private set; }
        public Color BorderColor { get; private set; }
        public string Text { get; set; }
        public string Displayed { get; set; } = "";
        public SpriteFont Font { get; set; }
        public Color Foreground { get; set; }
        public Xna.Vector2 Inside { get; set; }
        public float CharDelay { get; set; } = .05f;
        public float charWait { get; set; } = 0;
        // Private
        public Dialog(GuiManager gui, Xna.Vector2 location, Xna.Vector2 dimensions, Color color, Color foreground, string text, SpriteFont font, int border = 6, Color? borderColor = null) : base(location)
        {
            Gui = gui;
            Dimensions = dimensions;
            Color = color;
            Border = border;
            BorderColor = (borderColor == null ? Color.Black : (Color)borderColor);
            Text = text;
            Font = font;
            Foreground = foreground;

            // Other variables
            Inside = new(Dimensions.X - Border * 2 - 2, Dimensions.Y - Border * 2 - 2);
            Rect = new((int)Location.X, (int)Location.Y, (int)Dimensions.X, (int)Dimensions.Y);
        }
        public override void Update(float deltaTime)
        {
            if (Displayed.Length < Text.Length)
            {
                charWait -= deltaTime;
                if (charWait <= 0)
                {
                    Displayed += Text[Displayed.Length];
                    Displayed = SoftwrapWords(Displayed, Font, Inside);
                    charWait = CharDelay;
                }
            }
        }
        public override void Draw(SpriteBatch batch)
        {
            // Not drawing
            if (!IsVisible) { return; }

            // Background
            batch.FillRectangle(Rect, Color);
            // Text
            batch.DrawString(Font, LimitLines(Displayed, Font, Inside.Y), new(Location.X + Border + 2, Location.Y + Border + 2), Foreground);
            // Outline
            batch.DrawRectangle(Rect, BorderColor, Border);
        }
        public static string SoftwrapWords(string text, SpriteFont font, Xna.Vector2 dimensions)
        {
            // setup
            string wrapped = "";
            int start = 0;
            int end = 1;

            while (end < text.Length)
            {
                // Wrap
                if (font.MeasureString(text[start..end]).X + 2 > dimensions.X)
                {
                    int cutoff = text[start..end].LastIndexOf(' ') + start;
                    if (cutoff <= start) { cutoff = end; }
                    wrapped += $"{text[start..cutoff]}\n";
                    start = cutoff + 1; // Add one to ignore the space itself
                    end = cutoff + 2;
                }
                end++;
            }
            wrapped += text[start..];
            return wrapped;
        }
        // Trims and ellipses
        public static string LimitString(string text, SpriteFont font, float width)
        {
            // If it fits
            if (font.MeasureString(text).X < width) { return text; }

            // Cutting off
            int end = text.Length - 1;
            while (text[..end].Length > 0 && font.MeasureString($"{text[..end]}...").X > width) { end--; }
            return $"{text[..end]}...";
        }
        public static string LimitLines(string text, SpriteFont font, float height)
        {
            // Height
            float lineHeight = font.MeasureString("|").Y;

            int maxLines = (int)(height / lineHeight);
            if (text.Split('\n').Length <= maxLines) return text;
            else
                return string.Join('\n', text.Split('\n')[..maxLines]) + "...";
        }
    }
}
