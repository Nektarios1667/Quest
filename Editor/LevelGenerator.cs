using System.IO;
using System.Linq;

namespace Quest.Editor;
public class Terrain(params (float min, float max, TileTypeID tile)[] ranges)
{
    public (float min, float max, TileTypeID tile)[] Ranges { get; private set; } = ranges;
}
public class Structure(TileTypeID?[] tiles, Point size, TileTypeID? spawnTile)
{
    public TileTypeID?[] Tiles { get; private set; } = tiles;
    public TileTypeID? SpawnTile { get; private set; } = spawnTile;
    public Point Size { get; private set; } = size;
}
public class LevelGenerator
{
    private FastNoiseLite Noise { get; set; } = new();
    private int _seed;
    private Dictionary<string, Structure> Structures = new();
    private List<Rectangle> GeneratedStructures = [];
    public Terrain Terrain { get; set; }
    public Dictionary<string, Terrain> Terrains { get; set; } = new();
    public int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            Noise.SetSeed(value);
        }
    }
    public LevelGenerator(int seed, float freq)
    {
        Seed = seed;
        Noise.SetSeed(seed);
        Noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        Noise.SetFrequency(freq);

        // Read each structure
        foreach (string file in Directory.GetFiles("GameData/Structures", "*.qst"))
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(file);
            Structures[name] = ReadStructure(file);
        }

        // Read each terrain preset
        foreach (string file in Directory.GetFiles("GameData/Terrain", "*.qtr"))
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(file);
            Terrains[name] = ReadTerrainPreset(file);
        }
        Terrain = Terrains["Islands"];
    }
    public float GetNormNoise(int x, int y)
    {
        // Generate noise value in range [0, 1]
        float noiseValue = Noise.GetNoise(x, y);
        return (noiseValue + 1) / 2; // Normalize to [0, 1]
    }
    public float GetNormNoise(Point point) => GetNormNoise(point.X, point.Y);
    public float GetNoise(int x, int y)
    {
        // Generate noise value in range [-1, 1]
        return Noise.GetNoise(x, y);
    }
    public float GetNoise(Point point) => GetNoise(point.X, point.Y);
    public TileTypeID GetGeneratedTile(int x, int y, float value)
    {
        foreach (var (min, max, tile) in Terrain.Ranges)
        {
            // Check if value is in range
            if (value >= min && value < max)
                return tile;
        }
        return TileTypeID.Sky;
    }
    public TileTypeID GetGeneratedTile(Point point, float value) => GetGeneratedTile(point.X, point.Y, value);

    public Tile[] GenerateTerrain(int width, int height)
    {
        Tile[] level = new Tile[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Generate tile type based on noise value
                float value = GetNormNoise(x, y);
                TileTypeID tileType = GetGeneratedTile(x, y, value);
                level[y * width + x] = LevelManager.TileFromId(tileType, new(x, y));
            }
        }

        return level;
    }
    public Tile[] GenerateTerrain(Point size) => GenerateTerrain(size.X, size.Y);
    public Tile[] GenerateLevel(int width, int height, int structureAttempts)
    {
        // Terrain
        Tile[] level = GenerateTerrain(width, height);

        // Structures
        GeneratedStructures.Clear(); // Reset generated structures
        if (structureAttempts <= 0) return level;
        for (int i = 0; i < structureAttempts; i++)
        {
            // Randomly select a structure
            Structure structure = Structures.Values.ElementAt(RandomManager.RandomIntRange(0, Structures.Count - 1));
            Point spawnPoint = new(RandomManager.RandomIntRange(0, width - structure.Size.X), RandomManager.RandomIntRange(0, height - structure.Size.Y));

            // Check if on valid tile
            if (structure.SpawnTile != null && level[spawnPoint.Y * width + spawnPoint.X].Type.ID != structure.SpawnTile) continue;
            // Intersects other structures
            Rectangle rect = new(spawnPoint.X - 1, spawnPoint.Y - 1, structure.Size.X + 1, structure.Size.Y + 1);
            if (GeneratedStructures.Any(r => r.Intersects(rect))) continue; // Overlap

            // Place
            for (int y = 0; y < structure.Size.Y; y++)
            {
                for (int x = 0; x < structure.Size.X; x++)
                {
                    TileTypeID? tileType = structure.Tiles[y * structure.Size.X + x];
                    if (!tileType.HasValue) continue;
                    level[(spawnPoint.Y + y) * width + (spawnPoint.X + x)] = LevelManager.TileFromId(tileType.Value, new(spawnPoint.X + x, spawnPoint.Y + y));
                }
            }
            // Add to generated structures
            GeneratedStructures.Add(rect);
        }
        return level;
    }
    public Tile[] GenerateLevel(Point size, int structureAttempts) => GenerateLevel(size.X, size.Y, structureAttempts);
    public static Structure ReadStructure(string file)
    {
        // Check
        if (!file.EndsWith(".qst"))
        {
            Logger.Error($"Failed to read structure '{file}'. Expected .qst file.");
            return new([], Point.Zero, null);
        }
        if (!File.Exists(file)) Logger.Error($"File {file} not found.", true);

        // Setup
        List<TileTypeID?> tileTypesBuffer = [];
        TileTypeID? spawn;
        Point size = Point.Zero;
        using (FileStream stream = File.Open(file, FileMode.Open))
        using (BinaryReader reader = new(stream))
        {
            // Header
            byte b = reader.ReadByte();
            spawn = b == 0 ? null : (TileTypeID)(b - 1);
            size = new(reader.ReadByte(), reader.ReadByte());

            // Tiles
            for (int i = 0; i < size.X * size.Y; i++)
            {
                b = reader.ReadByte();
                TileTypeID? type = b == 0 ? null : (TileTypeID)(b - 1);
                tileTypesBuffer.Add(type);
            }
        }
        return new([.. tileTypesBuffer], size, spawn);
    }
    public static Terrain ReadTerrainPreset(string file)
    {
        // Check
        if (!file.EndsWith(".qtr"))
        {
            Logger.Error($"Failed to read preset '{file}'. Expected .qtr file.");
            return new();
        }
        if (!File.Exists(file)) Logger.Error($"File {file} not found.", true);

        // Read
        List<(float min, float max, TileTypeID tile)> ranges = [];
        using (FileStream stream = File.Open(file, FileMode.Open))
        using (BinaryReader reader = new(stream))
        {
            // Header
            int count = reader.ReadByte();
            // Ranges
            for (int i = 0; i < count; i++)
            {
                float min = reader.ReadByte() / 100f;
                float max = reader.ReadByte() / 100f;
                TileTypeID tile = (TileTypeID)reader.ReadByte();
                ranges.Add((min, max, tile));
            }
        }
        return new([.. ranges]);
    }
}
