using System;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Water : Tile
{
    public Water(Xna.Point location) : base(location)
    {
        IsWalkable = false;
    }
    public override void Draw(IGameManager game)
    {
        // Draw each tile using the sprite batch
        Point dest = Location * Constants.TileSize - game.Camera.ToPoint();
        dest += Constants.Middle;
        // Draw
        Color color = Marked ? Color.Red : Color.Lerp(Color.LightBlue, Color.Blue, 0.1f * (float)Math.Sin(game.Time + Location.X + Location.Y));
        DrawTexture(game.Batch, TextureID.Water, new Rectangle(dest, Constants.TileSize), source: game.TileTextureSource(this), color: color, scale: Constants.TileSizeScale.ToVector2());
        Marked = false; // Reset marked state for next frame
    }
}
