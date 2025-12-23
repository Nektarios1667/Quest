using System.Diagnostics;
using System.IO;
using IO = System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Quest.Managers;
public class LevelManager
{
    public List<ILootGenerator> LootGenerators = new();
    public List<Level> Levels { get; private set; }
    public Level Level { get; private set; }
    public Color SkyColor { get; set; }
    public event Action<string>? LevelLoaded;
    public static readonly Point lootStackOffset = new(4, 4);
    public static readonly Level EmptyLevel;
    static LevelManager()
    {
        Tile[] grassTiles = new Tile[256 * 256];
        for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++) grassTiles[t] = new Grass(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
        EmptyLevel = new("NUL/NUL", grassTiles, [], new(128, 128), [], [], [], [], []);
    }
    public LevelManager()
    {
        // Empty
        Levels = [];
        Level = EmptyLevel;
    }
    public void Update(GameManager gameManager)
    {
        if (!StateManager.IsPlayingState) return;

        // Entities
        foreach (NPC npc in Level.NPCs) npc.Update(gameManager);
        foreach (Enemy enemy in Level.Enemies) enemy.Update(gameManager);
        if (Level.Loot.Count > 255)
            Level.Loot.RemoveRange(0, Level.Loot.Count - 255);

        // SkyTint
        UpdateSky(gameManager);


        // Dynamic lighting
        foreach (Loot loot in Level.Loot)
            if (loot.Item.Type == ItemTypes.Lantern)
            {
                Point loc = loot.Location - CameraManager.Camera.ToPoint() + Constants.Middle + TextureManager.Metadata[loot.Texture].Size;
                LightingManager.SetLight($"Loot_{loot.UID}", loc, 2, Color.Transparent);
            }
    }
    public void UpdateSky(GameManager gameManager)
    {
        // Custom tint
        if (Level.Tint != Color.Transparent)
        {
            SkyColor = Level.Tint;
            return;
        }
        SkyColor = ColorTools.GetSkyColor(gameManager.DayTime) * 0.9f;
    }
    public Color GetWeatherColor(GameManager gameManager, Point loc, float? blend = null)
    {
        // Calculate sky colors from weather, biome, and time
        BiomeType? currentBiome = GetBiome(loc);
        blend ??= StateManager.WeatherIntensity(gameManager.GameTime);

        Color weatherColor = default;
        if (currentBiome == null || currentBiome == BiomeType.Indoors || blend == 0) weatherColor = Color.Transparent;
        else
        {
            switch (currentBiome)
            {
                case BiomeType.Temperate: weatherColor = Color.MediumBlue; break;
                case BiomeType.Snowy: weatherColor = Color.White; break;
                case BiomeType.Desert: weatherColor = Color.OrangeRed; break;
                case BiomeType.Ocean: weatherColor = Color.SlateGray; break;
            }
        }
        weatherColor *= blend.Value;
        return weatherColor;
    }
    public void Draw(GameManager gameManager)
    {
        if (!StateManager.IsPlayingState) return;

        DrawTiles(gameManager);
        DrawDecals(gameManager);
        DrawLoot(gameManager);
        DrawCharacters(gameManager);
    }
    public void DrawTiles(GameManager gameManager)
    {
        // Tiles
        DebugManager.StartBenchmark("TileDraws");
        if (Level.Tiles == null || Level.Tiles.Length == 0) return;

        // Get bounds
        Point start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;

        // Iterate
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                GetTile(x, y)?.Draw(gameManager);
            }
        }
        DebugManager.EndBenchmark("TileDraws");
    }
    public void DrawDecals(GameManager gameManager)
    {
        // Draw each decal
        DebugManager.StartBenchmark("DecalDraws");
        foreach (Decal decal in Level.Decals)
            decal.Draw(gameManager);
        DebugManager.EndBenchmark("DecalDraws");
    }
    public void DrawLoot(GameManager gameManager)
    {
        DebugManager.StartBenchmark("DrawLoot");
        // Draw each
        for (int l = 0; l < Level.Loot.Count; l++)
        {
            Loot loot = Level.Loot[l];
            Point pos = loot.Location - CameraManager.Camera.ToPoint() + Constants.Middle;
            pos.Y += (int)(Math.Sin((gameManager.GameTime - loot.Birth) * 2 % (Math.PI * 2)) * 6); // Bob up and down
            DrawTexture(gameManager.Batch, loot.Texture, pos, scale: 2);
            // Draw stacks if multiple
            if (loot.Item.Amount > 1)
                DrawTexture(gameManager.Batch, loot.Texture, pos + lootStackOffset, scale: 2);
            if (loot.Item.Amount > 2)
                DrawTexture(gameManager.Batch, loot.Texture, pos + lootStackOffset + lootStackOffset, scale: 2);
            // Draw hitbox if enabled
            if (DebugManager.DrawHitboxes)
                FillRectangle(gameManager.Batch, new(pos, new(32)), Constants.DebugPinkTint);
        }
        DebugManager.EndBenchmark("DrawLoot");
    }
    public void DrawCharacters(GameManager gameManager)
    {
        DebugManager.StartBenchmark("CharacterDraws");
        foreach (NPC npc in Level.NPCs) npc.Draw(gameManager);
        foreach (Enemy enemy in Level.Enemies) enemy.Draw(gameManager);
        DebugManager.EndBenchmark("CharacterDraws");
    }
    public Level GetLevel(string name)
    {
        foreach (Level level in Levels)
            if (level.Name == name)
                return level;
        Logger.Error($"Level '{name}' not found in stored levels.");
        return new("", [], [], new Point(128, 128), [], [], [], [], []);
    }
    public bool LoadLevel(GameManager gameManager, int levelIndex)
    {
        // Check index
        if (levelIndex < -Levels.Count || levelIndex >= Levels.Count)
        {
            Logger.Error("Invalid level index.");
            return false;
        }

        // Close dialogs
        if (Level != null && NPC.DialogBox != null)
        {
            NPC.DialogBox.IsVisible = false;
            NPC.DialogBox.Displayed = "";
        }

        // Load the level data
        if (levelIndex < 0) levelIndex = Levels.Count - Math.Abs(levelIndex);
        Level = Levels[levelIndex];

        // MiniMap
        gameManager.UIManager.RefreshMiniMap();

        // Run
        LevelLoaded?.Invoke(Level.Name);
        Level.RunScripts();

        // Spawn
        CameraManager.CameraDest = (Level.Spawn * Constants.TileSize).ToVector2();
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(0); // Ensure in bounds

        return true;
    }
    public bool LoadLevel(GameManager gameManager, string name)
    {
        name = name.Replace('\\', '/');
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name == name)
            {
                LoadLevel(gameManager, l);
                return true; ;
            }
        }
        // If not found throw an error
        Logger.Error($"Level '{name}' not found in stored levels.");
        return false;
    }
    public bool LoadLevelObject(GameManager gameManager, Level level)
    {
        // Close dialogs
        if (Level != null && NPC.DialogBox != null)
        {
            NPC.DialogBox.IsVisible = false;
            NPC.DialogBox.Displayed = "";
        }

        // Load the level data
        Level = level;

        // MiniMap
        gameManager.UIManager.RefreshMiniMap();

        // Spawn
        CameraManager.CameraDest = (Level.Spawn * Constants.TileSize).ToVector2();
        CameraManager.Camera = CameraManager.CameraDest;
        Logger.System($"Loaded level '{level.Name}'.");
        return true;
    }
    public bool UnloadLevel(int levelIndex)
    {
        // Check index
        if (levelIndex < 0 || levelIndex >= Levels.Count)
        {
            Logger.Error($"Invalid level index {levelIndex}.");
            return false;
        }

        string name = Levels[levelIndex].Name;
        if (Level == Levels[levelIndex]) Level = EmptyLevel;

        // Dispose
        Level level = Levels[levelIndex];
        for (int l = 0; l < level.Loot.Count; l++)
            level.Loot[l].Dispose();
        for (int e = 0; e < level.Enemies.Count; e++)
            level.Enemies[e].Dispose();
        UIDManager.ReleaseAll(UIDCategory.Items);

        // Remove
        Levels.Remove(level);
        Logger.System($"Unloaded level '{name}'.");
        return true;
    }
    public bool UnloadLevel(string levelName)
    {
        levelName = levelName.Replace('\\', '/');
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name != levelName) continue;
            UnloadLevel(l);
            return true;
        }

        Logger.Error($"Level '{levelName}' not found in stored levels.");
        return false;
    }
    public bool ReadWorld(GameManager gameManager, string folder, bool reload = false)
    {
        if (!Directory.Exists($"GameData/Worlds/{folder}"))
        {
            Logger.Error($"World '{folder}' does not exist.");
            return false;
        }
        FileTools.CheckDirExists($"GameData/Worlds/{folder}/loot");
        FileTools.CheckDirExists($"GameData/Worlds/{folder}/levels");

        // Read loot tables and presets
        string[] qlp = Directory.GetFiles($"GameData/Worlds/{folder}/loot", "*.qlp");
        string[] qlt = Directory.GetFiles($"GameData/Worlds/{folder}/loot", "*.qlt");
        foreach (string file in qlp.Concat(qlt).Select(f => IO.Path.GetFileName(f)))
        {
            LootGeneratorHelper.Read(folder, file);
            Logger.System($"Loaded Loot file {file}.");
        }
        // Read levels
        foreach (string file in Directory.GetFiles($"GameData/Worlds/{folder}/levels", "*.qlv"))
            ReadLevel(gameManager, $"{folder}/{IO.Path.GetFileNameWithoutExtension(file)}", reload);

        return true;
    }
    public bool UnloadWorld(string folder)
    {
        for (int l = Levels.Count - 1; l >= 0; l--)
            if (Levels[l].World == folder)
                UnloadLevel(l);
        return true;
    }
    private static bool Error(string message)
    {
        Logger.Error(message);
        return false;
    }
    public bool ReadLevel(GameManager gameManager, string filename, bool reload = false)
    {
        var sw = new Stopwatch();
        // File checks
        filename = filename.Replace('\\', '/');
        string[] splitPath = filename.Split('\\', '/');
        string path = $"GameData/Worlds/{splitPath[0]}/levels/{splitPath[1]}.qlv";

        if (splitPath.Length != 2) return Error($"Invalid file format '{filename}.'");
        if (!File.Exists(path)) return Error($"Level file '{filename}' does not exist.");

        // Check if already read
        if (!reload && Levels.Any(l => l.Name == filename)) return true;

        // Make buffers
        int totalTiles = Constants.MapSize.X * Constants.MapSize.Y;
        Tile[] tilesBuffer = new Tile[totalTiles];
        BiomeType[] biomeBuffer = new BiomeType[totalTiles];
        List<NPC> npcBuffer = [];
        List<Loot> lootBuffer = [];
        List<Decal> decalBuffer = [];
        List<QuillScript> scriptBuffer = [];

        // Context
        using FileStream fileStream = File.OpenRead(path);
        using BufferedStream buffer = new(fileStream, 8192);
        using GZipStream gzipStream = new(buffer, CompressionMode.Decompress);
        using BinaryReader reader = new(gzipStream);
        //try
        {

            // Metadata
            byte[] magic = reader.ReadBytes(4);
            if (Encoding.UTF8.GetString(magic) != "QLVL") return Error($"Invalid file format for file '{filename}'.");
            LevelFeatures flags = (LevelFeatures)reader.ReadUInt16();

            // Tint
            Color tint = reader.ReadColor();
            // Spawn
            Point spawn = reader.ReadByteCoord().ToPoint();

            // Tiles
            for (int y = 0; y < Constants.MapSize.Y; y++)
                for (int x = 0; x < Constants.MapSize.X; x++)
                    tilesBuffer[x + y * Constants.MapSize.X] = ReadTile(reader, flags, splitPath, x, y);

            // Biomes
            if (flags.HasFlag(LevelFeatures.Biomes))
            {
                int read = reader.Read(MemoryMarshal.AsBytes(biomeBuffer.AsSpan()));
                if (read != totalTiles) Error($"Failed to read biome data for level '{filename}' - expected {totalTiles}B got {read}B.");
            }
            else
                biomeBuffer = [];

            // NPCs
            int npcCount = reader.ReadByte();
            for (int n = 0; n < npcCount; n++)
                npcBuffer.Add(reader.ReadNPC(gameManager));

            // Loot
            byte lootCount = reader.ReadByte();
            for (int n = 0; n < lootCount; n++)
                lootBuffer.Add(reader.ReadLoot(gameManager));

            // Decals
            byte decalCount = reader.ReadByte();
            for (int n = 0; n < decalCount; n++)
                decalBuffer.Add(reader.ReadDecal());

            // Scripts
            if (flags.HasFlag(LevelFeatures.QuillScripts))
                for (int s = 0; s < reader.ReadByte(); s++)
                    scriptBuffer.Add(new QuillScript(reader.ReadString(), reader.ReadString()));

            // Make and add the level
            Level created = new(filename, tilesBuffer, biomeBuffer, spawn, npcBuffer, lootBuffer, decalBuffer, [], scriptBuffer, tint);
            if (reload) Levels.RemoveAll(l => l.Name == filename);
            Levels.Add(created);
            sw.Stop();
            Logger.System($"Successfully read level '{filename}' in {sw.ElapsedMilliseconds:F2}s.");
            return true;
        }
        //catch (Exception ex)
        //{
        //    Logger.Error($"Failed to read level file '{filename}': {ex.Message}");
        //    return false;
        //}
    }
    private static Tile ReadTile(BinaryReader reader, LevelFeatures flags, string[] splitPath, int x, int y)
    {
        // Helper
        Door ReadDoor(Point loc)
        {
            string keyName = reader.ReadString();
            return new Door(loc, keyName.IsNUL() ? null : new(ItemTypes.Get(keyName), reader.ReadByte()));
        }
        Chest ReadChest(Point loc)
        {
            string lootGenFile = reader.ReadString();
            ILootGenerator lootGen = LootGeneratorHelper.Read(splitPath[0], lootGenFile);
            if (lootGen.FileName.IsNUL() || lootGen.FileName == "_")
                return new Chest(loc, LootPreset.EmptyPreset, splitPath[1]);
            else
                return new Chest(loc, lootGen, splitPath[1]);
        }

        // Read tile data
        Point loc = new(x, y);
        if (!Enum.TryParse(reader.ReadByte().ToString(), out TileTypeID type)) return Error($"Invalid tile type at {x}, {y} in level file.") ? new Grass(loc) : new Grass(loc);

        return type switch
        {
            TileTypeID.Stairs => new Stairs(loc, $"{splitPath[0]}/{reader.ReadString()}", new(reader.ReadByte(), reader.ReadByte())),
            TileTypeID.Door => ReadDoor(loc),
            TileTypeID.Chest => ReadChest(loc),
            TileTypeID.Lamp => new Lamp(loc, reader.ReadByte()),
            _ => new Tile(loc, type),
        };
    }

    public static Tile TileFromId(int id, ByteCoord location) => TileFromId(id, location);
    public static Tile TileFromId(int id, Point location)
    {
        // Create a tile from an id
        TileTypeID type = (TileTypeID)id;
        return type switch
        {
            TileTypeID.Stairs => new Stairs(location, "", Constants.MiddleCoord),
            TileTypeID.Door => new Door(location, null),
            TileTypeID.Chest => new Chest(location, LootPreset.EmptyPreset, "_"),
            TileTypeID.Lamp => new Lamp(location),
            _ => new(location, type)
        };
    }
    public static Tile TileFromId(TileTypeID id, Point location) => TileFromId((int)id, location);
    public static Decal DecalFromId(DecalType id, Point location) => new(location, id);
    public int TileConnectionsMask(Tile tile)
    {
        int mask = 0;
        int x = tile.X;
        int y = tile.Y;

        Tile? left = GetTile(x - 1, y);
        Tile? right = GetTile(x + 1, y);
        Tile? up = GetTile(x, y - 1);
        Tile? down = GetTile(x, y + 1);

        if (left == null || left.Type == tile.Type || (tile.IsWall && left.IsWall)) mask |= 1; // left
        if (down == null || down.Type == tile.Type || (tile.IsWall && down.IsWall)) mask |= 2; // down
        if (right == null || right.Type == tile.Type || (tile.IsWall && right.IsWall)) mask |= 4; // right
        if (up == null || up.Type == tile.Type || (tile.IsWall && up.IsWall)) mask |= 8; // up

        return mask;
    }
    public int BiomeConnectionsMask(Point loc)
    {
        int mask = 0;
        int x = loc.X;
        int y = loc.Y;

        BiomeType? center = GetBiome(x, y);
        BiomeType? left = GetBiome(x - 1, y);
        BiomeType? right = GetBiome(x + 1, y);
        BiomeType? up = GetBiome(x, y - 1);
        BiomeType? down = GetBiome(x, y + 1);

        if (center == null) return mask;
        if (left == null || left == center) mask |= 1; // left
        if (down == null || down == center) mask |= 2; // down
        if (right == null || right == center) mask |= 4; // right
        if (up == null || up == center) mask |= 8; // up

        return mask;
    }
    public Rectangle TileTextureSource(Tile tile)
    {

        int mask = TileConnectionsMask(tile);

        int srcX = mask % Constants.TileMapDim.X * Constants.TilePixelSize.X;
        int srcY = mask / Constants.TileMapDim.X * Constants.TilePixelSize.Y;

        return new(srcX, srcY, Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
    }
    public Rectangle BiomeTextureSource(Point loc)
    {

        int mask = BiomeConnectionsMask(loc);

        int srcX = mask % Constants.TileMapDim.X * Constants.TilePixelSize.X;
        int srcY = mask / Constants.TileMapDim.X * Constants.TilePixelSize.Y;

        return new(srcX, srcY, Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
    }
    public BiomeType? GetBiome(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            return null;
        return Level.Biome[coord.X + coord.Y * Constants.MapSize.X];
    }
    public BiomeType? GetBiome(int x, int y)
    {
        if (x < 0 || x >= Constants.MapSize.X || y < 0 || y >= Constants.MapSize.Y)
            return null;
        return Level.Biome[x + y * Constants.MapSize.X];
    }
    public Tile? GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            return null;
        return Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public Tile? GetTile(int x, int y)
    {
        if (x < 0 || x >= Constants.MapSize.X || y < 0 || y >= Constants.MapSize.Y)
            return null;
        return Level.Tiles[x + y * Constants.MapSize.X];
    }
    public void DropLoot(GameManager gameManager, Loot loot)
    {
        Level.Loot.Add(loot);
        gameManager.UIManager.LootNotifications.AddNotification($"-{loot.DisplayName}");
    }
    public static Point TileCoord(Vector2 loc) => new((int)(loc.X / Constants.TileSize.X), (int)(loc.Y / Constants.TileSize.Y));
    public static Vector2 WorldCoord(Point tileCoord) => new(tileCoord.X * Constants.TileSize.X, tileCoord.Y * Constants.TileSize.Y);
    public static int Flatten(Point point) => point.X + point.Y * Constants.MapSize.X;
}
