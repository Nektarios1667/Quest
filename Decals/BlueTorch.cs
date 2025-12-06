namespace Quest.Decals;
public class BlueTorch(Point location) : Decal(location)
{
    private static readonly Vector2 lightShift = new(0, 1);
    public override void Draw(GameManager game)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        dest += (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap) / Constants.TwoPoint - TextureManager.Metadata[TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        DrawTexture(game.Batch, TextureID.Glow, dest, scale: Constants.TileSizeScale, color: Color.Cyan * ((float)Math.Cos(game.GameTime) / 8 + .4f));

        LightingManager.SetLight($"BlueTorchDecal_{X}_{Y}", (Location.ToVector2() + lightShift).ToPoint() * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle, 5, new(0, 50, 50, 60));

        base.Draw(game);
    }
}