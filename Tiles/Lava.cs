namespace Quest.Tiles;

public class Lava : Tile, IDynamicTile
{
    public Lava(Point location) : base(location, TileTypeID.Lava)
    { }
    public override void Draw(GameManager gameManager)
    {
        // Draw each tile using the sprite batch
        Point dest = CameraManager.TileToScreen(Location);

        dest += Constants.Middle;
        // Draw tile
        Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, 0.5f * (float)Math.Sin(GameManager.GameTime * MathHelper.PiOver2));
        Rectangle source = new((int)((Math.Cos(GameManager.GameTime * 0.1f) + 1) / 2 * 48), (int)((Math.Sin(GameManager.GameTime * 0.2f) + 1) / 2 * 48), Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
        DrawTexture(gameManager.Batch, TextureID.Lava, dest, source: source, color: color, scale: Constants.TileSizeScale);
        DrawTexture(gameManager.Batch, TextureID.LavaBorder, dest, source: gameManager.LevelManager.TileTextureSource(this), color: color, scale: Constants.TileSizeScale);
        LightingManager.SetLight($"Lava_{X}_{Y}", Location.ToPoint(), 4, singleFrame: true);
    }
}
