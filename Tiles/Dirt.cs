namespace Quest.Tiles;

public class Dirt : Tile
{
    public Dirt(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
