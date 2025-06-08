using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class StoneWall : Tile
{
    public StoneWall(Xna.Point location) : base(location)
    {
        IsWalkable = false;
    }
}
