namespace Quest.Tiles;

public class Path : Tile
{
    public Path(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
