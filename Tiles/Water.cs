using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SharpDX.Direct2D1.Effects;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Quest.Tiles
{
    public class Water : Tile
    {
        public Water(Xna.Point location) : base(location)
        {
            IsWalkable = false;
        }
        public override void Draw(GameHandler game)
        {
            // Draw each tile using the sprite batch
            Vector2 dest = Location.ToVector2() * Constants.TileSize - game.Camera;
            dest += Constants.Middle;
            // Draw
            Texture2D? texture = game.TileTextures.TryGetValue(Type.ToString(), out var tex) ? tex : null;
            Color color = Marked ? Color.Red : Color.Lerp(Color.LightBlue, Color.Blue, 0.1f * (float)Math.Sin(game.Time + Location.X + Location.Y));
            Marked = false; // Reset marked state for next frame
            game.TryDraw(texture, new Rectangle(dest.ToPoint(), Constants.TileSize.ToPoint()), game.TileTextureSource(this), color, 0f, Vector2.Zero, Constants.TileSizeScale);
        }
    }
}
