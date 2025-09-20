using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Lava : Tile
{
    public Lava(Point location) : base(location)
    {
        IsWalkable = false;
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw each tile using the sprite batch
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint();

        dest += Constants.Middle;
        // Draw tile
        Color color = Marked ? Color.Red : Color.Lerp(Color.Yellow, Color.OrangeRed, 0.5f * (float)Math.Sin(gameManager.GameTime * MathHelper.PiOver2));
        Rectangle source = new((int)((Math.Cos(gameManager.GameTime * 0.1f) + 1) / 2 * 48), (int)((Math.Sin(gameManager.GameTime * 0.2f) + 1) / 2 * 48), Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
        DrawTexture(gameManager.Batch, TextureID.Lava, dest, source: source, color: color, scale: Constants.TileSizeScale);

        Marked = false; // Reset marked state for next frame
    }
}
