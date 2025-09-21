namespace Quest.Tiles;

public class StoneTiles : Tile
{
    public StoneTiles(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
