using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Grass : Tile
{
    public Grass(Xna.Point location) : base(location)
    {
        IsWalkable = true;
    }
}
