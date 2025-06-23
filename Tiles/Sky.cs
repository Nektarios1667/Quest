namespace Quest.Tiles;

public class Sky : Tile
{
    public Sky(Point location) : base(location)
    {
        IsWalkable = false;
    }
}
