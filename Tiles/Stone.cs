namespace Quest.Tiles;

public class Stone : Tile
{
    public Stone(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
