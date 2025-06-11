using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Darkness : Tile
{
    public Darkness(Point location) : base(location)
    {
        IsWalkable = false;
    }
}
