namespace Quest.Tiles;

public class Snow : Tile, ICold
{
    public Snow(Point location) : base(location, TileTypes.Snow)
    {
    }
}
