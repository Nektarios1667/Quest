using HarfBuzzSharp;
using MonoGame.Extended.Screens.Transitions;
using System.Linq;
using System.Threading;

namespace Quest.World;

public class WorldMetadata
{
    public static WorldMetadata Null => new("Unknown", "None");
    public string Author { get; set; }
    public string Description { get; set; }
    public WorldMetadata(string author, string description)
    {
        Author = author;
        Description = description;
    }
    public Dictionary<string, string> ToDict() => new()
    {
        { "Author", Author  },
        { "Description", Description },
    };
}

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
    public ushort UID;
    public Dictionary<ushort, Enemy> Enemies { get; private set; }
    public List<Projectile> Projectiles { get; private set; }
    public List<Waypoint> Waypoints { get; private set; } = [];
    public Dictionary<ByteCoord, Decal> Decals { get; private set; }
    public Dictionary<ushort, NPC> NPCs { get; private set; }
    public LevelPath LevelPath { get; private set; }
    public string Path => LevelPath.Path;
    public string WorldName => LevelPath.WorldName;
    public string LevelName => LevelPath.LevelName;
    public WorldMetadata Metadata { get; private set; }
    public List<Loot> Loot { get; private set; }
    public Tile[] Tiles { get; private set; }
    public List<LevelTransition> Transitions { get; private set; }
    public bool[] Explored { get; private set; } = new bool[Constants.MapSize.X * Constants.MapSize.Y];
    public BiomeType[] Biome { get; private set; }
    public Point Spawn { get; set; }
    public Color Tint { get; set; }
    public List<QuillScript> Scripts { get; private set; }
    public Level(string name, Tile[] tiles, BiomeType[] biomes, Point spawn, WorldMetadata meta, ushort? uid = null)
    {
        // Initialize the level
        LevelPath = new(name);
        Tiles = tiles;
        Biome = biomes.Length == 0 ? new BiomeType[Constants.MapSize.X * Constants.MapSize.Y] : biomes;
        Spawn = spawn;
        NPCs = [];
        Loot = [];
        Decals = [];
        Enemies = [];
        Projectiles = [];
        Transitions = [];
        Scripts = [];
        Metadata = meta;
        Tint = Color.Transparent;
        UID = uid ?? UIDManager.Get(UIDCategory.Levels);
    }
    public Level(string name, Tile[] tiles, BiomeType[] biome, Point spawn, List<NPC> npcs, List<Loot> loot, Dictionary<ByteCoord, Decal> decals, List<Enemy> enemies, List<Projectile> projectiles, List<LevelTransition> transitions, List<QuillScript> scripts, WorldMetadata meta, Color? tint = null, ushort? uid = null)
    {
        // Initialize the level
        LevelPath = new(name);
        Tiles = tiles;
        Biome = biome.Length == 0 ? new BiomeType[Constants.MapSize.X * Constants.MapSize.Y] : biome;
        Spawn = spawn;
        NPCs = npcs.ToDictionary(npc => npc.UID, npc => npc);
        Loot = [.. loot];
        Decals = decals;
        Enemies = enemies.ToDictionary(enemy => enemy.UID, enemy => enemy);
        Projectiles = [.. projectiles];
        Transitions = [.. transitions];
        Scripts = [.. scripts];
        Metadata = meta;
        Tint = tint ?? Color.Transparent;
        UID = uid ?? UIDManager.Get(UIDCategory.Levels);
    }
    public void RunScripts()
    {
        foreach (var script in Scripts)
        {
            Quill.Interpreter.RunScript(script);
        }
    }
    public void Rename(LevelPath path) => LevelPath = path;
    public void AddWaypoint(Waypoint point) {
        if (Waypoints.Count >= byte.MaxValue)
        {
            Logger.Error($"Level {Path} at maxiumum waypoint count (255).");
            return;
        }
        Waypoints.Add(point);
    }
    public void AddWaypoints(Waypoint[] points)
    {
        int addAmount = Math.Min(points.Length, byte.MaxValue - Waypoints.Count);
        Waypoints.AddRange(points[..addAmount]);

        // Checks
        if (addAmount != points.Length)
            Logger.Error($"Level {Path} at maxiumum waypoint count (255). {points.Length - addAmount} waypoints not added.");
    }
    public bool RemoveWaypoint(string name) => Waypoints.RemoveAll(p => p.Name == name) > 0;
    public bool RemoveWaypoint(Waypoint point) => Waypoints.Remove(point);
}
