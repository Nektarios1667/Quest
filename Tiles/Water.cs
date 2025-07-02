using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Water : Tile
{
    public Water(Xna.Point location) : base(location)
    {
        IsWalkable = false;
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw each tile using the sprite batch
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint();

        dest += Constants.Middle;
        // Draw tile
        Color color = Marked ? Color.Red : Color.Lerp(Color.LightBlue, Color.Blue, 0.1f * (float)Math.Sin(gameManager.GameTime + Location.X + Location.Y));
        DrawTexture(gameManager.Batch, TextureID.Water, dest, source: gameManager.LevelManager.TileTextureSource(this), color: color, scale: Constants.TileSizeScale);

        // Lighting
        float distSq = Vector2.DistanceSquared(dest.ToVector2() + Constants.HalfVec, Constants.Middle.ToVector2() + CameraManager.CameraOffset - Constants.MageHalfSize.ToVector2()) / (Constants.TileSize.X * Constants.TileSize.Y);
        Color lighting = Color.Lerp(Color.Transparent, gameManager.LevelManager.SkyLight, Math.Clamp(distSq / 25, 0, 1));
        gameManager.Batch.FillRectangle(new(dest.ToVector2(), Constants.TileSize), lighting);

        Marked = false; // Reset marked state for next frame
    }
}
