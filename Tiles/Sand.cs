using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class Sand : Tile
{
    public Sand(Xna.Point location) : base(location)
    {
        IsWalkable = true;
    }
}
