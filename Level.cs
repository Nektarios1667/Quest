namespace Quest;

public class QuillScript
{
    public string Name { get; private set; }
    public string SourceCode { get; private set; }
    public QuillScript(string scriptName, string sourceCode)
    {
        Name = scriptName;
        SourceCode = sourceCode;
    }
}

public class Level
{
    public List<Enemy> Enemies { get; private set; }
    public List<Projectile> Projectiles { get; private set; }
    public Dictionary<ByteCoord, Decal> Decals { get; private set; }
    public List<NPC> NPCs { get; private set; }
    public LevelPath LevelPath { get; private set; }
    public string Path => LevelPath.Path;
    public string WorldName => LevelPath.WorldName;
    public string LevelName => LevelPath.LevelName;
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public BiomeType[] Biome { get; private set; }
    public Point Spawn { get; set; }
    public Color Tint { get; set; }
    public List<QuillScript> Scripts { get; private set; }
    public Level(string name, Tile[] tiles, BiomeType[] biome, Point spawn, List<NPC> npcs, List<Loot> loot, Dictionary<ByteCoord, Decal> decals, List<Enemy> enemies, List<Projectile> projectiles, List<QuillScript> scripts, Color? tint = null)
    {
        // Initialize the level
        LevelPath = new(name);
        Tiles = tiles;
        Biome = biome.Length == 0 ? new BiomeType[Constants.MapSize.X * Constants.MapSize.Y] : biome;
        Spawn = spawn;
        NPCs = [.. npcs];
        Loot = [.. loot];
        Decals = decals;
        Enemies = [.. enemies];
        Projectiles = [.. projectiles];
        Scripts = [.. scripts];
        Tint = tint ?? Color.Transparent;
    }
    public void RunScripts()
    {
        foreach (var script in Scripts)
        {
            Quill.Interpreter.RunScript(script);
        }
    }
    public void Rename(LevelPath path) => LevelPath = path;
}
