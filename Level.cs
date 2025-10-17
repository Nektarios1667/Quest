using System.Security.Policy;

namespace Quest;

public class Level
{
    public List<Enemy> Enemies { get; private set; }
    public List<Decal> Decals { get; private set; }
    public List<NPC> NPCs { get; private set; }
    public string Name { get; private set; }
    public string World { get; private set; }
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public BiomeType[] Biome { get; private set; }
    public Point Spawn { get; set; }
    public Color Tint { get; set; }
    public Level(string name, Tile[] tiles, BiomeType[] biome, Point spawn, List<NPC> npcs, List<Loot> loot, List<Decal> decals, List<Enemy> enemies, Color? tint = null)
    {
        // Initialize the level
        Name = name;
        World = name.Split('\\', '/')[0];
        Tiles = tiles;
        Biome = biome.Length == 0 ? new BiomeType[Constants.MapSize.X * Constants.MapSize.Y] : biome;
        Spawn = spawn;
        NPCs = [.. npcs];
        Loot = [.. loot];
        Decals = [.. decals];
        Enemies = [.. enemies];
        Tint = tint ?? Color.Transparent;
    }
}
