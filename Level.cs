namespace Quest;

public class QuillScript
{
    public string ScriptName { get; private set; }
    public string SourceCode { get; private set; }
    public QuillScript(string scriptName, string sourceCode)
    {
        ScriptName = scriptName;
        SourceCode = sourceCode;
    }
}

public class Level
{
    public List<Enemy> Enemies { get; private set; }
    public List<Decal> Decals { get; private set; }
    public List<NPC> NPCs { get; private set; }
    public LevelPath LevelPath { get; private set; }
    public string Name => LevelPath.Path;
    public string World => LevelPath.WorldName;
    public string LevelName => LevelPath.LevelName;
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public BiomeType[] Biome { get; private set; }
    public Point Spawn { get; set; }
    public Color Tint { get; set; }
    public List<QuillScript> Scripts { get; private set; }
    public Level(string name, Tile[] tiles, BiomeType[] biome, Point spawn, List<NPC> npcs, List<Loot> loot, List<Decal> decals, List<Enemy> enemies, List<QuillScript> scripts, Color? tint = null)
    {
        // Initialize the level
        LevelPath = new(name);
        Tiles = tiles;
        Biome = biome.Length == 0 ? new BiomeType[Constants.MapSize.X * Constants.MapSize.Y] : biome;
        Spawn = spawn;
        NPCs = [.. npcs];
        Loot = [.. loot];
        Decals = [.. decals];
        Enemies = [.. enemies];
        Scripts = [.. scripts];
        Tint = tint ?? Color.Transparent;
    }
    public void RunScripts()
    {
        foreach (var script in Scripts)
        {
            _ = Quill.Interpreter.RunScriptAsync(script);
        }
    }
}
