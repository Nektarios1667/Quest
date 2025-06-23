namespace Quest.Entities;
public class BlueTorch(Point location) : Decal(location)
{
    public override void Draw(GameManager game)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        dest += (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap) / Constants.TwoPoint - TextureManager.Metadata[TextureManager.TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        TextureManager.DrawTexture(game.Batch, TextureManager.TextureID.Glow, dest, scale: 4, color: Color.Cyan * ((float)Math.Cos(game.TotalTime) / 8 + .4f));
        base.Draw(game);
    }
}
