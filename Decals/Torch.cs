namespace Quest.Decals;
public class Torch(Point location) : Decal(location)
{
    public override void Draw(GameManager game)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        dest += (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap) / Constants.TwoPoint - TextureManager.Metadata[TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        Tint = Color.Lerp(Color.Red, Color.LightYellow, (float)Math.Sin(game.TotalTime) + .5f);
        DrawTexture(game.Batch, TextureID.Glow, dest, scale: 4, color: Color.Lerp(Tint, Color.DarkOrange, .8f) * ((float)Math.Cos(game.TotalTime) / 8 + .4f));
        base.Draw(game);
    }
}
