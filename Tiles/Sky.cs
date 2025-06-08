using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Sky : Tile
{
    public Sky(Xna.Point location) : base(location)
    {
        IsWalkable = false;
    }
}
