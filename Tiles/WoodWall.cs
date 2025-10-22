namespace Quest.Tiles;

public class WoodWall : Tile
{
    public WoodWall(Point location) : base(location)
    {
        IsWalkable = false;
        IsWall = true;
    }
}
