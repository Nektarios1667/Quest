using System.IO;

namespace Quest.Editor;
public class Structure(TileType?[] tiles, Point size, TileType? spawnTile)
{
    public TileType?[] Tiles { get; private set; } = tiles;
    public TileType? SpawnTile { get; private set; } = spawnTile;
    public Point Size { get; private set; } = size;
}
public class LevelGenerator
{
    private FastNoiseLite Noise { get; set; } = new();
    private int _seed;
    private Structure[] structures = new Structure[Directory.GetFiles("Structures").Length];
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

        // Setup structures
        List<TileType?> tileTypesBuffer = [];
        int f = 0;
        Point size = Point.Zero;
        TileType? spawn = null;

        // Read each structure
        foreach (string file in Directory.GetFiles("Structures"))
        {
            tileTypesBuffer.Clear();
            using (FileStream stream = File.Open(file, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                // Header
                byte b = reader.ReadByte();
                spawn = b == 0 ? null : (TileType)b;
                size = new(reader.ReadByte(), reader.ReadByte());

                // Tiles
                for (int i = 0; i < size.X * size.Y; i++)
                {
                    b = reader.ReadByte();
                    TileType? type = b == 0 ? null : (TileType)b;
                    tileTypesBuffer.Add(type);
                }
            }
            structures[f] = new([.. tileTypesBuffer], size, spawn);
            f++;
        }
    }

    public Dictionary<TileType, (float min, float max)> TileRanges { get; set; } = new()
    {
        { TileType.Water, (0, 0.58f) },
        { TileType.Sand, (.58f, .6f) },
        { TileType.Grass, (.6f, 1) },
    };
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
    public TileType GetGeneratedTile(int x, int y, float? value = null)
    {
        value = value ?? GetNormNoise(x, y); // Noise
        foreach (var (tileType, (min, max)) in TileRanges)
        {
            if (value >= min && value < max)
                return tileType;
        }
        return TileType.Sky;
    }
    public TileType GetGeneratedTile(Point point, float? value = null) => GetGeneratedTile(point.X, point.Y, value);

    public Tile[] GenerateTerrain(int width, int height)
    {
        Tile[] level = new Tile[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Generate tile type based on noise value
                float value = GetNormNoise(x, y);
                TileType tileType = GetGeneratedTile(x, y, value);
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
        if (structureAttempts <= 0) return level;
        for (int i = 0; i < structureAttempts; i++)
        {
            // Randomly select a structure
            Structure structure = structures[RandomManager.RandomIntRange(0, structures.Length - 1)];
            Point spawnPoint = new(RandomManager.RandomIntRange(0, width - structure.Size.X), RandomManager.RandomIntRange(0, height - structure.Size.Y));

            // Check if on valid tile
            if (structure.SpawnTile != null && level[spawnPoint.Y * width + spawnPoint.X].Type != structure.SpawnTile) continue;

            // Place
            for (int y = 0; y < structure.Size.Y; y++)
            {
                for (int x = 0; x < structure.Size.X; x++)
                {
                    TileType? tileType = structure.Tiles[y * structure.Size.X + x];
                    if (!tileType.HasValue) continue;
                    level[(spawnPoint.Y + y) * width + (spawnPoint.X + x)] = LevelManager.TileFromId(tileType.Value, new(spawnPoint.X + x, spawnPoint.Y + y));
                }
            }
        }
        return level;
    }
    public Tile[] GenerateLevel(Point size, int structureAttempts) => GenerateLevel(size.X, size.Y, structureAttempts);

}
