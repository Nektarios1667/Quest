using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Quest.TextureManager;

namespace Quest.Tiles
{
    public class Water : Tile
    {
        public Water(Xna.Point location) : base(location)
        {
            IsWalkable = false;
        }
        public override void Draw(IGameManager game)
        {
            // Draw each tile using the sprite batch
            Vector2 dest = Location.ToVector2() * Constants.TileSize - game.Camera;
            dest += Constants.Middle;
            // Draw
            Color color = Marked ? Color.Red : Color.Lerp(Color.LightBlue, Color.Blue, 0.1f * (float)Math.Sin(game.Time + Location.X + Location.Y));
            DrawTexture(game.Batch, TextureID.Water, new Rectangle(dest.ToPoint(), Constants.TileSize.ToPoint()), source:game.TileTextureSource(this), color:color, scale:Constants.TileSizeScale);
            Marked = false; // Reset marked state for next frame
        }
    }
}
