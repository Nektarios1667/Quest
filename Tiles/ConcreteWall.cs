using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class ConcreteWall : Tile
{
    public ConcreteWall(Xna.Point location) : base(location)
    {
        IsWalkable = false;
    }
}
