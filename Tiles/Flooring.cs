using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Flooring : Tile
{
    public Flooring(Xna.Point location) : base(location)
    {
        IsWalkable = true;
    }
}
