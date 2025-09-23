using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class StoneWall : Tile
{
    public StoneWall(Point location) : base(location)
    {
        IsWalkable = false;
        IsWall = true;
    }
}
