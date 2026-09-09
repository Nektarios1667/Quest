namespace Quest.World;

public class Waypoint
{
    public ByteCoord Position { get; set; }
    public string Name { get; set; }
    public Color Color { get; set; }
    public bool PlayerMade { get; set; }
    public Waypoint(ByteCoord pos, string name, Color color, bool playerMade = false)
    {
        Position = pos;
        Name = name;
        Color = color;
        PlayerMade = playerMade;
    }
}
