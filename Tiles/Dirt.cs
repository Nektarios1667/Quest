using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Dirt : Tile
{
    public Dirt(Xna.Point location) : base(location)
    {
        IsWalkable = true;
    }
}
