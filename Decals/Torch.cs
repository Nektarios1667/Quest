namespace Quest.Decals;
public class Torch(Point location) : Decal(location)
{
    public override void Draw(GameManager game)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        dest += (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap) / Constants.TwoPoint - TextureManager.Metadata[TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        Tint = Color.Lerp(Color.Red, Color.LightYellow, (float)Math.Sin(game.GameTime) + .5f);
        DrawTexture(game.Batch, TextureID.Glow, dest, scale: Constants.TileSizeScale, color: Color.Lerp(Tint, Color.DarkOrange, .8f) * ((float)Math.Cos(game.GameTime) / 8 + .4f));

        LightingManager.SetLight($"TorchDecal_{Location.X}_{Location.Y}", Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle, 300, new(30, 8, 0, 200), 0.8f);

        base.Draw(game);
    }
}
