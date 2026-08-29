using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.World;

public class Waypoint
{
    public LevelPath LevelPath { get; set; }
    public ByteCoord Position { get; set; }
    public string Name { get; set; }
    public Color Color { get; set; }
    public bool PlayerMade { get; set; }
    public Waypoint(LevelPath level, ByteCoord pos, string name, Color color, bool playerMade = false)
    {
        LevelPath = level;
        Position = pos;
        Name = name;
        Color = color;
        PlayerMade = playerMade;
    }
}
