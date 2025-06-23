namespace Quest.Tiles;

public class Flooring : Tile
{
    public Flooring(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
