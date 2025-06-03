using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Gui
{
    public class StatusBar : Widget
    {
        public int MaxValue { get; set; }
        public int CurrentValue { get; set; }
        public Xna.Point Size { get; set; }
        public Xna.Color Foreground { get; set; }
        public Xna.Color Background { get; set; }
        public StatusBar(Xna.Vector2 location, Xna.Point size, Xna.Color foreground, Xna.Color background, int currentValue, int maxValue) : base(location)
        {
            Size = size;
            CurrentValue = currentValue;
            MaxValue = maxValue;
            Foreground = foreground;
            Background = background;
        }
        public override void Draw(SpriteBatch batch)
        {
            batch.FillRectangle(new(Location, Size), Background); // Background
            batch.FillRectangle(new(Location.X, Location.Y, Size.X * CurrentValue / MaxValue, Size.Y), Foreground); // Foreground
        }
    }
}
