using System;

namespace Quest.Entities;
public class Torch(Point location) : Decal(location)
{
    public override void Draw(IGameManager game)
    {
        Point dest = Location * Constants.TileSize - game.Camera.ToPoint() + Constants.Middle;
        dest += (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap) / Constants.TwoPoint - TextureManager.Metadata[TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        Tint = Color.Lerp(Color.Red, Color.LightYellow, (float)Math.Sin(game.Time) + .5f);
        DrawTexture(game.Batch, TextureID.Glow, dest, scale: new(4), color: Color.Lerp(Tint, Color.DarkOrange, .8f) * ((float)Math.Cos(game.Time) / 8 + .4f));
        base.Draw(game);
    }
}
