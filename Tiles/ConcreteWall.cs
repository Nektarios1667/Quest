using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class ConcreteWall : Tile
{
    public ConcreteWall(Point location) : base(location)
    {
        IsWalkable = false;
        IsWall = true;
    }
}
