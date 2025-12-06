namespace Quest.Tiles;

public class Ice : Tile, ICold
{
    public Ice(Point location) : base(location, TileTypeID.Ice)
    {
    }
}
