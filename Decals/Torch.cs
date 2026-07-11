namespace Quest.Decals;

public class Torch(Point location) : Decal(location)
{
    public static readonly Point lightShift = new(0, 1);
    public override void Draw(GameManager game)
    {
        Point dest = CameraManager.TileToScreen(Location);
        dest += TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap / Constants.TwoPoint - TextureManager.Metadata[TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        Color tint = Color.Lerp(Color.Red, Color.LightYellow, (float)Math.Sin(GameManager.GameTime) + .5f);
        DrawTexture(game.Batch, TextureID.Glow, dest, scale: Constants.TileSizeScale, color: Color.Lerp(tint, Color.DarkOrange, .8f) * ((float)Math.Cos(GameManager.GameTime) / 8 + .4f));

        LightingManager.SetLight($"TorchDecal_{X}_{Y}", Location + lightShift, 6, singleFrame: true);

        base.Draw(game);
    }
}
