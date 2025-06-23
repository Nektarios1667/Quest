namespace Quest.Tiles;

public class Grass : Tile
{
    public Grass(Point location) : base(location)
    {
        IsWalkable = true;
    }
}
