using Quest.Enemies;

namespace Quest;

public class Level
{
    public List<Enemy> Enemies { get; private set; }
    public List<Decal> Decals { get; private set; }
    public List<NPC> NPCs { get; private set; }
    public string Name { get; private set; }
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public Point Spawn { get; set; }
    public Color Tint { get; set; }
    public Level(string name, Tile[] tiles, Point spawn, List<NPC> npcs, List<Loot> loot, List<Decal> decals, List<Enemy> enemies, Color? tint = null)
    {
        // Initialize the level
        Name = name;
        Tiles = tiles;
        Spawn = spawn;
        NPCs = [.. npcs];
        Loot = [.. loot];
        Decals = [.. decals];
        Enemies = [.. enemies];
        Tint = tint ?? Color.Transparent;
    }
}
