namespace Quest;


public class Level
{
    public Enemy[] Enemies { get; set; }
    public Decal[] Decals { get; set; }
    public NPC[] NPCs { get; private set; }
    public string Name { get; private set; }
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public Point Spawn { get; private set; }
    public Color Tint { get; private set; }
    public Level(string name, Tile[] tiles, Point spawn, NPC[] npcs, Loot[] loot, Decal[] decals, Enemy[] enemies, Color? tint = null)
    {
        // Initialize the level
        Name = name;
        Tiles = tiles;
        Spawn = spawn;
        NPCs = npcs;
        Loot = [.. loot];
        Decals = decals;
        Enemies = enemies;
        Tint = tint ?? Color.Transparent;
    }
}
