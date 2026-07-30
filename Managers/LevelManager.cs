using Quest.Quill;
using Quest.World;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IO = System.IO;

namespace Quest.Managers;


public class LevelManager
{
    // Loading
    public event Action<float>? LoadingProgressed;
    public int TotalTasks { get; set; }
    private int _tasksComplete;
    public int TasksComplete
    {
        get { return _tasksComplete; }
        set { _tasksComplete = value; LoadingProgressed?.Invoke(LoadingProgress); }
    }
    public float LoadingProgress => TotalTasks <= 0 ? 0 : (float)TasksComplete / TotalTasks;
    // Level data
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
        EmptyLevel = new("NUL/NUL", grassTiles, [], new(128, 128), [], [], [], [], [], [], WorldMetadata.Null);
    }
    public LevelManager()
    {
        // Empty
        Levels = [];
        Level = EmptyLevel;
        TimerManager.SetTimer("UpdatePathfindingGrid", 1f, () =>
            PathfindingManager.SetGrid(Level,
                CameraManager.TopLeftTileCoord - Constants.TileDrawPadding,
                Constants.NativeResolutionTiles + Constants.TileDrawPadding.Scaled(2)
            ),
            int.MaxValue
        );
    }
    public void Update(GameManager gameManager)
    {
        if (!StateManager.IsPlayingState) return;

        // Entities
        DebugManager.StartBenchmark("LevelEntityUpdates");
        foreach (NPC npc in Level.NPCs.Values) npc.Update(gameManager);
        var enemyList = Level.Enemies.Values.ToArray();
        for (int p = enemyList.Length - 1; p >= 0; p--)
        {
            enemyList[p].Update(gameManager);
            if (!enemyList[p].IsAlive) Level.Enemies.Remove(enemyList[p].UID);
        }
        for (int p = Level.Projectiles.Count - 1; p >= 0; p--)
        {
            Level.Projectiles[p].Update(gameManager);
            if (!Level.Projectiles[p].IsAlive) Level.Projectiles.RemoveAt(p);
        }
        if (Level.Loot.Count >= ushort.MaxValue)
            Level.Loot.RemoveRange(0, Level.Loot.Count - ushort.MaxValue);
        DebugManager.EndBenchmark("LevelEntityUpdates");

        // SkyTint
        UpdateSky(gameManager);


        // Dynamic lighting
        DebugManager.StartBenchmark("LootLighting");
        foreach (Loot loot in Level.Loot)
        {
            if (loot.Item.Type == ItemTypes.Lantern)
            {
                Point loc = CameraManager.WorldToTile(loot.Position + TextureManager.Metadata[loot.Texture].Size);
                LightingManager.SetLight($"Loot_{loot.UID}", loc, 2);
            }
        }
        DebugManager.EndBenchmark("LootLighting");
    }
    public void UpdateSky(GameManager gameManager)
    {
        // Custom tint
        if (Level.Tint != Color.Transparent)
        {
            SkyColor = Level.Tint;
            return;
        }
        SkyColor = WeatherManager.GetSkyColor(gameManager.DayTime) * 0.9f;
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

        // Get bounds - padding start includes the padding area, while screen start is the area that is actually visible on screen
        Point paddingStart = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize - Constants.TileDrawPadding;
        Point paddingEnd = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize + Constants.TileDrawPadding;
        Point screenStart = paddingStart + Constants.TileDrawPadding;
        Point screenEnd = paddingEnd - Constants.TileDrawPadding;

        // Iterate through each tile in the padded bounds
        for (int y = paddingStart.Y; y <= paddingEnd.Y; y++)
        {
            for (int x = paddingStart.X; x <= paddingEnd.X; x++)
            {
                // Get tile
                var tile = GetTile(x, y);
                if (tile == null) continue;

                // Cull static offscreen tiles - either it has to be on screen, or a dynamic tile e.g. lamp that emits light even offscreen
                bool onScreen = x >= screenStart.X && x <= screenEnd.X && y >= screenStart.Y && y <= screenEnd.Y;
                if (onScreen || tile is IDynamicTile)
                    tile.Draw(gameManager);
            }
        }
        DebugManager.EndBenchmark("TileDraws");
    }
    public void DrawDecals(GameManager gameManager)
    {
        // Draw each decal
        DebugManager.StartBenchmark("DecalDraws");
        foreach (Decal decal in Level.Decals.Values)
            decal.Draw(gameManager);
        DebugManager.EndBenchmark("DecalDraws");
    }
    public void DrawLoot(GameManager gameManager)
    {
        DebugManager.StartBenchmark("DrawLoot");
        // Draw each loot
        foreach (Loot loot in Level.Loot)
            loot.Draw(gameManager);
        DebugManager.EndBenchmark("DrawLoot");
    }
    public void DrawCharacters(GameManager gameManager)
    {
        DebugManager.StartBenchmark("CharacterDraws");
        foreach (NPC npc in Level.NPCs.Values) npc.Draw(gameManager);
        foreach (Enemy enemy in Level.Enemies.Values) enemy.Draw(gameManager);
        foreach (Projectile projectile in Level.Projectiles) projectile.Draw(gameManager);
        DebugManager.EndBenchmark("CharacterDraws");
    }
    public Level GetLevel(string name)
    {
        foreach (Level level in Levels)
            if (level.Path == name)
                return level;
        Logger.Error($"Level '{name}' not found in stored levels.");
        return new("", [], [], new Point(128, 128), [], [], [], [], [], [], WorldMetadata.Null);
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

        return LoadLevelObject(gameManager, Levels[levelIndex]);
    }
    public bool LoadLevel(GameManager gameManager, string name)
    {
        name = name.Replace('\\', '/');
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Path == name)
            {
                LoadLevel(gameManager, l);
                return true;
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
        gameManager.OverlayManager?.RefreshMiniMap();

        // Pathfinding
        PathfindingManager.SetGrid(Level,
            CameraManager.TopLeftTileCoord - Constants.TileDrawPadding,           // Start
            Constants.NativeResolutionTiles + Constants.TileDrawPadding.Scaled(2) // Size
        );

        // Lighting
        LightingManager.ClearLights();
        LightingManager.BuildLevelLighting(gameManager);

        // Spawn
        CameraManager.CameraDest = (Level.Spawn * Constants.TileSize).ToVector2();
        CameraManager.Camera = CameraManager.CameraDest;
        Logger.System($"Loaded level '{level.Path}'.");
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

        string name = Levels[levelIndex].Path;
        if (Level == Levels[levelIndex]) Level = EmptyLevel;

        // Dispose
        Level level = Levels[levelIndex];
        foreach (Loot loot in level.Loot)
            loot.Dispose();
        foreach (Enemy enemy in level.Enemies.Values)
            enemy.Dispose();
        UIDManager.ReleaseAll(UIDCategory.Items);

        // Stop Quill scripts
        Interpreter.ClearScripts();

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
            if (Levels[l].Path != levelName) continue;
            UnloadLevel(l);
            return true;
        }

        Logger.Error($"Level '{levelName}' not found in stored levels.");
        return false;
    }
    public async Task<bool> ReadWorldAsync(GameManager gameManager, string filename, bool reload = false)
    {
        return await Task.Run(() =>
        {
            return ReadWorld(gameManager, filename, reload);
        });
    }
    public bool ReadWorld(GameManager gameManager, string folder, bool reload = false)
    {
        string[] levelFiles = Directory.GetFiles($"GameData/Worlds/{folder}/levels", "*.qlv");

        // Progress info
        TasksComplete = 0;
        TotalTasks = levelFiles.Length * 11 + 1; // All levels * 11 tasks per level + reading loot files
        MenuManager.SetCurrentlyLoading("Loading world...");

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
        TasksComplete++;

        // Read levels
        foreach (string file in levelFiles)
        {
            MenuManager.SetCurrentlyLoading($"Loading {IO.Path.GetFileNameWithoutExtension(file)}...");
            ReadLevel(gameManager, $"{folder}/{IO.Path.GetFileNameWithoutExtension(file)}", reload, multiTask: true);
            TasksComplete++;
        }

        return true;
    }
    public bool UnloadWorld(string folder)
    {
        for (int l = Levels.Count - 1; l >= 0; l--)
            if (Levels[l].WorldName == folder)
                UnloadLevel(l);
        return true;
    }
    private static bool Error(string message)
    {
        Logger.Error(message);
        return false;
    }
    public async Task<bool> ReadLevelAsync(GameManager gameManager, string filename, bool reload = false)
    {
        return await Task.Run(() =>
        {
            return ReadLevel(gameManager, filename, reload);
        });
    }
    public bool ReadLevel(GameManager gameManager, string filename, bool reload = false, bool multiTask = false)
    {
        if (!multiTask)
        {
            TasksComplete = 0;
            TotalTasks = 12;
        }

        var sw = new Stopwatch();
        sw.Start();
        // File checks
        filename = filename.Replace('\\', '/');
        LevelPath levelPath = new(filename);
        string path = $"GameData/Worlds/{levelPath.WorldName}/levels/{levelPath.LevelName}.qlv";

        if (levelPath.IsNull()) return Error($"Invalid file format '{filename}.'");
        if (!File.Exists(path)) return Error($"Level file '{filename}' does not exist.");

        // Read metadata
        WorldMetadata meta = WorldMetadata.Null;
        var kvDict = StateManager.ReadKeyValueFile($"Worlds/{levelPath.WorldName}/metadata");
        meta.Author = kvDict.GetValueOrDefault("Author", defaultValue: "Unknown");
        meta.Description = kvDict.GetValueOrDefault("Description", defaultValue: "None");
        TasksComplete++;

        // Check if already read
        if (!reload && Levels.Any(l => l.Path == filename)) return true;

        // Make buffers
        int totalTiles = Constants.MapSize.X * Constants.MapSize.Y;
        Tile[] tilesBuffer = new Tile[totalTiles];
        BiomeType[] biomeBuffer = new BiomeType[totalTiles];
        Point spawn = new();
        Color tint = new();
        List<Loot> lootBuffer = new();
        List<NPC> npcBuffer = new();
        List<Enemy> enemyBuffer = new();
        Dictionary<ByteCoord, Decal> decalBuffer = new();
        List<QuillScript> scriptBuffer = new();

        TasksComplete++;

        // Context
        using FileStream fileStream = File.OpenRead(path);
        using BufferedStream buffer = new(fileStream, 128 * 1024);
        using GZipStream gzipStream = new(buffer, CompressionMode.Decompress);
        using BinaryReader reader = new(gzipStream);

        // Metadata
        byte[] magic = reader.ReadBytes(4);
        if (Encoding.ASCII.GetString(magic) != "QLVL") return Error($"invalid file format for file '{filename}'.");
        TasksComplete++;

        // Reading
        try
        {
            // Read sections
            while (true)
            {
                string id = reader.ReadString();
                int length = reader.ReadInt32();


                if (id == "_EOF") break;

                byte[] data = reader.ReadBytes(length);

                using MemoryStream sectionStream = new MemoryStream(data);
                using BinaryReader sectionReader = new BinaryReader(sectionStream);

                // Section types
                switch (id)
                {
                    case "LEVL": ReadLevelSection(sectionReader); break;
                    case "TILE": ReadTileSection(sectionReader, levelPath, tilesBuffer); break;
                    case "BIOM": ReadBiomeSection(sectionReader, filename, totalTiles, biomeBuffer); break;
                    case "NPCS": ReadNPCSection(sectionReader, npcBuffer); break;
                    case "LOOT": ReadLootSection(sectionReader, gameManager, lootBuffer); break;
                    case "DCAL": ReadDecalSection(sectionReader, gameManager, decalBuffer); break;
                    case "ENEM": ReadEnemySection(sectionReader, enemyBuffer); break;
                    case "QSCR": ReadScriptSection(sectionReader, levelPath, scriptBuffer); break;
                    default: Logger.Warning($"Unknown level section '{id}'"); break; // Unknown section - ignore it
                }
            }

            // Make and add the level
            Level created = new(filename, tilesBuffer, biomeBuffer, spawn, npcBuffer, lootBuffer, decalBuffer, enemyBuffer, [], scriptBuffer, meta, tint);
            if (reload) Levels.RemoveAll(l => l.Path == filename);
            Levels.Add(created);
            sw.Stop();
            Logger.System($"Successfully read level '{filename}' in {sw.ElapsedMilliseconds:F0}ms.");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to read level file '{filename}': {ex}");
            return false;
        }

    }
    private void ReadLevelSection(BinaryReader reader)
    {
        // Tint
        Color tint = reader.ReadColor();
        TasksComplete++;

        // Spawn
        Point spawn = reader.ReadByteCoord().ToPoint();
        TasksComplete++;
    }
    private void ReadTileSection(BinaryReader reader, LevelPath levelPath, Tile[] tilesBuffer)
    {
        // Tiles
        for (int y = 0; y < Constants.MapSize.Y; y++)
            for (int x = 0; x < Constants.MapSize.X; x++)
                tilesBuffer[x + y * Constants.MapSize.X] = ReadTile(reader, levelPath, x, y);
        TasksComplete++;
    }
    private void ReadBiomeSection(BinaryReader reader, string filename, int totalTiles, BiomeType[] biomeBuffer)
    {
        // Biomes
        int read = reader.Read(MemoryMarshal.AsBytes(biomeBuffer.AsSpan()));
        if (read != totalTiles) Error($"Failed to read biome data for level '{filename}' - expected {totalTiles}B got {read}B.");
        TasksComplete++;
    }
    private void ReadNPCSection(BinaryReader reader, List<NPC> npcBuffer)
    {
        // NPCs
        ushort npcCount = reader.ReadUInt16();
        npcBuffer.Clear();
        npcBuffer.Capacity = npcCount;

        for (int n = 0; n < npcCount; n++)
            npcBuffer.Add(reader.ReadNPC());
        TasksComplete++;
    }
    private void ReadLootSection(BinaryReader reader, GameManager gameManager, List<Loot> lootBuffer)
    {
        // Loot
        ushort lootCount = reader.ReadUInt16();
        lootBuffer.Clear();
        lootBuffer.Capacity = lootCount;

        for (int n = 0; n < lootCount; n++)
            lootBuffer.Add(reader.ReadLoot(gameManager));
        TasksComplete++;
    }
    private void ReadDecalSection(BinaryReader reader, GameManager gameManager, Dictionary<ByteCoord, Decal> decalBuffer)
    {
        // Decals
        ushort decalCount = reader.ReadUInt16();
        decalBuffer.Clear();

        for (int n = 0; n < decalCount; n++)
        {
            Decal decal = reader.ReadDecal();
            decalBuffer[decal.Location] = decal;
        }
        TasksComplete++;
    }
    private void ReadEnemySection(BinaryReader reader, List<Enemy> enemyBuffer)
    {
        // Enemies
        ushort enemyCount = reader.ReadUInt16();
        enemyBuffer.Clear();
        enemyBuffer.Capacity = enemyCount;
        
        for (int e = 0; e < enemyCount; e++)
            enemyBuffer.Add(reader.ReadEnemy());
        TasksComplete++;
    }
    private void ReadScriptSection(BinaryReader reader, LevelPath levelPath, List<QuillScript> scriptBuffer)
    {
        // Scripts
        Directory.CreateDirectory($"GameData/Worlds/{levelPath.WorldName}/scripts");

        byte scriptCount = reader.ReadByte();
        scriptBuffer.Clear();
        scriptBuffer.Capacity = scriptCount;


        for (int s = 0; s < scriptCount; s++)
        {
            string name = reader.ReadString();
            string scriptPath = $"GameData/Worlds/{levelPath.WorldName}/scripts/{name}";
            string code = File.Exists(scriptPath) ? File.ReadAllText(scriptPath) : "// NUL";
            scriptBuffer.Add(new QuillScript(name, code));
        }

        TasksComplete++;
    } 
    private static Tile ReadTile(BinaryReader reader, LevelPath levelPath, int x, int y)
    {
        // Helpers
        Chest ReadChest(Point loc)
        {
            string lootGenFile = reader.ReadString();
            ILootGenerator lootGen = LootGeneratorHelper.Read(levelPath.WorldName, lootGenFile);
            lootGen = (lootGen.FileName.IsNUL() || lootGen.FileName == "_") ? LootPreset.EmptyPreset : lootGen;

            return new Chest(loc, lootGen, levelPath.LevelName, StateManager.ReadItemData(reader)?.GetItemRef(), reader.ReadBoolean());
        }
        DisplayCase ReadDisplayCase(Point loc)
        {
            Item? item = StateManager.ReadItemData(reader);
            DisplayCase displayCase = new(loc, levelPath.LevelName);
            displayCase.Container.Items[0] = item;
            return displayCase;
        }

        // Read tile data
        Point loc = new(x, y);
        if (!Enum.TryParse(reader.ReadByte().ToString(), out TileTypeID type)) return Error($"Invalid tile type at {x}, {y} in level file.") ? new Grass(loc) : new Grass(loc);

        return type switch
        {
            TileTypeID.Stairs => new Stairs(loc, new LevelPath(levelPath.WorldName, reader.ReadString()), new(reader.ReadByte(), reader.ReadByte())),
            TileTypeID.Door => new Door(loc, StateManager.ReadItemData(reader)?.GetItemRef(), reader.ReadBoolean()),
            TileTypeID.Chest => ReadChest(loc),
            TileTypeID.Lamp => new Lamp(loc, reader.ReadByte()),
            TileTypeID.DisplayCase => ReadDisplayCase(loc),
            _ => Tile.TileFromId(type, loc, levelPath.LevelName),
        };
    }
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

        if (left == null || left.TypeID == TileTypeID.Darkness || left.Type == tile.Type || (tile.IsWall && left.IsWall)) mask |= 1; // left
        if (down == null || down.TypeID == TileTypeID.Darkness || down.Type == tile.Type || (tile.IsWall && down.IsWall)) mask |= 2; // down
        if (right == null || right.TypeID == TileTypeID.Darkness || right.Type == tile.Type || (tile.IsWall && right.IsWall)) mask |= 4; // right
        if (up == null || up.TypeID == TileTypeID.Darkness || up.Type == tile.Type || (tile.IsWall && up.IsWall)) mask |= 8; // up

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
    public Decal? GetDecal(ByteCoord coord)
    {
        if (Level.Decals.TryGetValue(coord, out var dec)) return dec;
        return null;
    }
    public void DropLoot(GameManager gameManager, Loot loot)
    {
        Level.Loot.Add(loot);
        gameManager.OverlayManager.LootNotifications.AddNotification($"-{loot.DisplayName}");
    }
    public static int Flatten(Point point) => point.X + point.Y * Constants.MapSize.X;
}
