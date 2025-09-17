using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles;

public class WoodWall : Tile
{
    public WoodWall(Xna.Point location) : base(location)
    {
        IsWalkable = false;
    }
}
