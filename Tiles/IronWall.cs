namespace Quest.Tiles;

public class IronWall : Tile
{
    public IronWall(Point location) : base(location)
    {
        IsWalkable = false;
        IsWall = true;
    }
}
