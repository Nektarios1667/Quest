using System;

namespace Quest.Entities;
public class BlueTorch(Point location) : Decal(location)
{
    public override void Draw(IGameManager game)
    {
        Point dest = Location * Constants.TileSize - game.Camera.ToPoint() + Constants.Middle;
        dest += (TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap) / Constants.TwoPoint - TextureManager.Metadata[TextureID.Glow].Size / Constants.TwoPoint + new Point(0, -15);
        DrawTexture(game.Batch, TextureID.Glow, new(dest, TextureManager.Metadata[TextureID.Glow].Size), scale: new(4), color: Color.Cyan * ((float)Math.Cos(game.Time) / 8 + .4f));
        base.Draw(game);
    }
}
