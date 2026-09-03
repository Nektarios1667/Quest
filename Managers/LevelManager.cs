using Quest.Quill;
using Quest.World;
using System.Linq;

namespace Quest.Managers;


public class LevelManager
{
    // Loading
    public event Action<float>? LoadingProgressed;
    public event Action<Level>? LevelLoaded;
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
    public static readonly Point lootStackOffset = new(4, 4);
    private static Tile[] grassTiles = new Tile[256 * 256];
    public static Level EmptyLevel => new("NUL/NUL", grassTiles, [], new(128, 128), WorldMetadata.Null);
    static LevelManager()
    {
        for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++) grassTiles[t] = new Grass(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
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
        if (!gameManager.StateManager.IsPlayingState) return;

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
        SkyColor = WeatherManager.GetSkyColor(GameManager.DayTime) * 0.9f;
    }
    public void Draw(GameManager gameManager)
    {
        if (!gameManager.StateManager.IsPlayingState) return;

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
        Point paddingStart = CameraManager.TopLeftTileCoord - Constants.TileDrawPadding;
        Point paddingEnd = CameraManager.BottomRightTileCoord + Constants.TileDrawPadding;
        Point screenStart = CameraManager.TopLeftTileCoord;
        Point screenEnd = CameraManager.BottomRightTileCoord;

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
    public Level GetLevel(LevelPath level) => GetLevel(level.ToString());
    public Level GetLevel(string name)
    {
        foreach (Level level in Levels)
            if (level.Path == name)
                return level;
        Logger.Error($"Level '{name}' not found in stored levels.");
        return EmptyLevel;
    }
    public bool LoadLevel(GameManager gameManager, int levelIndex)
    {
        // Check index
        if (levelIndex < -Levels.Count || levelIndex >= Levels.Count)
        {
            Logger.Error("Invalid level index.");
            return false;
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
        gameManager.OverlayManager?.InvalidateMinimap();

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
        CameraManager.Update(gameManager, 0f); // Force update to avoid visual glitches

        Logger.System($"Loaded level '{level.Path}'.");
        LevelLoaded?.Invoke(Level);

        return true;
    }
    public bool UnloadWorld(string folder)
    {
        for (int l = Levels.Count - 1; l >= 0; l--)
            if (Levels[l].WorldName == folder)
                UnloadLevel(l);
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
    public Tile? GetTile(Point coord) => GetTile(Level, coord);
    public Tile? GetTile(int x, int y) => GetTile(Level, x, y);
    public Tile? GetTile(LevelPath level, Point coord) => GetTile(GetLevel(level), coord);
    public Tile? GetTile(LevelPath level, int x, int y) => GetTile(GetLevel(level), x, y);
    public static Tile? GetTile(Level level, Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y ||
        level.Tiles.Length < coord.X + coord.Y * Constants.MapSize.X)
            return null;
        return level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public static Tile? GetTile(Level level, int x, int y)
    {
        if (x < 0 || x >= Constants.MapSize.X || y < 0 || y >= Constants.MapSize.Y)
            return null;
        return level.Tiles[x + y * Constants.MapSize.X];
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
