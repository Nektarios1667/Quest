namespace Quest.Tiles;

public class Water : Tile
{
    public Water(Point location) : base(location, TileTypeID.Water)
    {
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw each tile using the sprite batch
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;

        // Draw tile
        Color color = Color.Lerp(Color.LightBlue, Color.Blue, 0.1f * (float)Math.Sin(gameManager.GameTime + X + Y));
        DrawTexture(gameManager.Batch, TextureID.Water, dest, source: gameManager.LevelManager.TileTextureSource(this), color: color, scale: Constants.TileSizeScale);
    }
}
